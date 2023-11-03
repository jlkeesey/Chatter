// Copyright 2023 James Keesey
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS �AS IS�
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Chatter.Chat;
using Chatter.Localization;
using Chatter.Model;
using Chatter.Reporting;
using Chatter.System;
using Chatter.Windows;
using Dalamud.Interface.Internal;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiScene;
using System.IO;
using System.Reflection;

namespace Chatter;

/// <summary>
///     The entry point for this plugin.
/// </summary>
public sealed partial class Chatter : IDalamudPlugin
{
    public static string Version = string.Empty;

    private readonly ChatLogManager _chatLogManager;
    private readonly ChatManager _chatManager;
    private readonly ICommandManager _commandManager;
    private readonly Configuration _configuration;
    private readonly ILogger _logger;
    private readonly JlkWindowManager _windowManager;
    private readonly IDalamudTextureWrap _chatterImage;

    public Chatter([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
                   [RequiredVersion("1.0")] IChatGui chatGui,
                   [RequiredVersion("1.0")] IClientState clientState,
                   [RequiredVersion("1.0")] ICommandManager commandManager,
                   [RequiredVersion("1.0")] IDataManager gameData)
    {
        _commandManager = commandManager;

        try
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            _logger = new Logger();

#if DEBUG
            SeSpecialCharacters.InitializeDebug(_logger);
#endif
            var loc = new Loc();
            loc.Load(_logger);

            var fileHelper = new FileHelper(new WindowsFileSystem());

            _configuration = Configuration.Load(pluginInterface, fileHelper);

            var dateManager = new DateHelper() as IDateHelper;
            var worldManager = new WorldManager(gameData);
            var myself = new Myself(clientState, worldManager);
            var friendManager = new FriendManager(worldManager);
            var errorWriter = new ChatErrorWriter(chatGui);
            var chatLogGenerator = new ChatLogGenerator(errorWriter);

            _chatLogManager = new ChatLogManager(_configuration, dateManager, fileHelper, myself, chatLogGenerator);
            _chatManager = new ChatManager(_configuration,
                                           _logger,
                                           _chatLogManager,
                                           chatGui,
                                           dateManager,
                                           myself.HomeWorld.Name);

            _chatterImage = LoadImage(pluginInterface, "chatter.png");
            _windowManager = new JlkWindowManager(pluginInterface,
                                                  _configuration,
                                                  dateManager,
                                                  friendManager,
                                                  Name,
                                                  _chatterImage,
                                                  loc);
            RegisterCommands();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public string Name => "Chatter";

    /// <summary>
    ///     Disposes all of the resources created by the plugin.
    /// </summary>
    /// <remarks>
    ///     In theory all of these objects are non-null so they all exist by this point in code, however, there is
    ///     a try/catch block around the initialization code which means that the constructor can fail before
    ///     reaching this point. That means that some of these objects are actually null at this point so we
    ///     add a null check to all of them just in case. If this were an application we could ignore them as the
    ///     system would clean up regardless, but as we are a DLL that is loaded into an application, the clean up
    ///     will not occur until that application (Dalamud) exits.
    /// </remarks>
    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _configuration?.Save(); // Should be auto-saved but let's be sure

        UnregisterCommands();
        _windowManager?.Dispose();
        _chatterImage?.Dispose();
        _chatLogManager?.Dispose();
        _chatManager?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }

    /// <summary>
    ///     Loads an image from the game assembly relative to the project root.
    /// </summary>
    /// <param name="pluginInterface">The controlling <see cref="DalamudPluginInterface" />.</param>
    /// <param name="name">The image file name relative to the project root.</param>
    /// <returns>The <see cref="TextureWrap" /> representing the image.</returns>
    private static IDalamudTextureWrap LoadImage(DalamudPluginInterface pluginInterface, string name)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var imagePath = Path.Combine(Path.GetDirectoryName(assemblyLocation)!, name);
        return pluginInterface.UiBuilder.LoadImage(imagePath);
    }
}
