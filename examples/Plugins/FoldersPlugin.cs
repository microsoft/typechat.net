// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;

namespace Plugins;

public class FoldersPlugin
{
    [KernelFunction("myPicturesFolder")]
    public string Pictures()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    }

    [KernelFunction("myDocumentsFolder")]
    public string Documents()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    [KernelFunction("myVideosFolder")]
    public string Videos()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    }

    [KernelFunction("myMusicFolder")]
    public string Music()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    }
}
