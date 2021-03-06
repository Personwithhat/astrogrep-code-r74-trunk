#!/bin/bash
echo "Making AstroGrep.exe ..."
mcs -target:winexe -pkg:gtk-sharp-2.0 -out:AstroGrep.exe \
    -win32icon:Images/AstroGrep_Icon.ico \
    AstroGrepBase/Grep.cs \
        AstroGrepBase/HitObject.cs AstroGrepBase/IAstroGrepPlugin.cs \
        AstroGrepBase/MicrosoftWordPlugin.cs \
        AstroGrepBase/PluginCollection.cs \
        AstroGrepBase/PluginWrapper.cs \
    Core/Common.cs Core/Constants.cs Core/GeneralSettings.cs \
        Core/PluginManager.cs Core/PluginSettings.cs Core/SearchSettings.cs \
        Core/SettingsIO.cs Core/TextEditor.cs \
    Linux/Forms/frmAbout.cs Linux/Forms/frmErrorLog.cs Linux/Forms/frmMain.cs \
        Linux/Forms/frmOptions.cs Linux/Common.cs Linux/Language.cs \
        Linux/Program.cs Linux/Controls/LinkLabel.cs \
    AssemblyInfo.cs \
    -resource:Images/AstroGrep_Banner.png,AstroGrep.Images.AstroGrep_Banner.png \
    -resource:Images/AstroGrep_Icon.ico,AstroGrep.Images.AstroGrep_Icon.ico \
    -resource:Images/options-general.png,AstroGrep.Images.options-general.png \
    -resource:Images/options-text-editors.png,AstroGrep.Images.options-text-editors.png \
    -resource:Images/options-results.png,AstroGrep.Images.options-results.png \
    -resource:Images/options-plugins.png,AstroGrep.Images.options-plugins.png \
    -resource:Language/Deutsch.xml,AstroGrep.Language.Deutsch.xml \
    -resource:Language/English.xml,AstroGrep.Language.English.xml \
    -resource:Language/Italiano.xml,AstroGrep.Language.Italiano.xml
