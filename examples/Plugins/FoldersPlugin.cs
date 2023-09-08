// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.SkillDefinition;

namespace Plugins;

public class FoldersPlugin
{
    [SKFunction, SKName("myPicturesFolder")]
    public string Pictures()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    }

    [SKFunction, SKName("myDocumentsFolder")]
    public string Documents()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    [SKFunction, SKName("myVideosFolder")]
    public string Videos()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    }

    [SKFunction, SKName("myMusicFolder")]
    public string Music()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    }
}
