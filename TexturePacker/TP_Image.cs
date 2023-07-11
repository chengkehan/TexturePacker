using System;
using System.Collections.Generic;
using UnityEngine;

public class TP_Image
{
    public int id;
    public int width;
    public int height;
    public int x;
    public int y;
    public int atlasIndex = -1;
    public int padding = 0;

    public TP_Image(int id, int width, int height, int padding)
    {
        this.id = id;
        this.width = width + padding * 2;
        this.height = height + padding * 2;
        this.padding = padding;
    }
}
