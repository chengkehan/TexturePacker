using System;
using System.Collections.Generic;

public class TP_Node
{
    public TP_Node[] child = new TP_Node[2];                      
    public TP_PixRect pixRect;
    public TP_Image img;

    bool isLeaf()
    {
        if (child[0] == null || child[1] == null)
        {
            return true;
        }
        return false;
    }

    public TP_Node Insert(TP_Image img, bool handed)
    {
        int a = 0, b = 0;
        if (handed == true) {
            a = 0;
            b = 1;
        }
        else if (handed == false) {
            a = 1;
            b = 0;
        }

        if (isLeaf() == false) {
            TP_Node newNode = child[a].Insert(img, handed);
            if (newNode != null)
                return newNode;
            return child[b].Insert(img, handed);
        }
        else {

            if (this.img != null)
                return null;

            if (pixRect.width < img.width || pixRect.height < img.height)
                return null;

            if (pixRect.width == img.width && pixRect.height == img.height)
            {
                this.img = img;
                return this;
            }

            child[a] = new TP_Node();
            child[b] = new TP_Node();

            int dw = pixRect.width - img.width;
            int dh = pixRect.height - img.height;

            if (dw > dh)
            {
                child[a].pixRect = new TP_PixRect(pixRect.x, pixRect.y, img.width, pixRect.height);
                child[b].pixRect = new TP_PixRect(pixRect.x + img.width, pixRect.y, pixRect.width - img.width, pixRect.height);
            }
            else
            {
                child[a].pixRect = new TP_PixRect(pixRect.x, pixRect.y, pixRect.width, img.height);
                child[b].pixRect = new TP_PixRect(pixRect.x, pixRect.y + img.height, pixRect.width, pixRect.height - img.height);
            }
            return child[a].Insert(img, handed);
        }
    }
}
