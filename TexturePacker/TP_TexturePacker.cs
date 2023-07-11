using System;
using System.Collections.Generic;
using UnityEngine;

public class TP_TexturePacker
{
    class ImgIDComparer : IComparer<TP_Image>
    {
        public int Compare(TP_Image x, TP_Image y)
        {
            if (x.id > y.id)
                return 1;
            if (x.id == y.id)
                return 0;
            return -1;
        }
    }

    class ImageHeightComparer : IComparer<TP_Image>
    {
        public int Compare(TP_Image x, TP_Image y)
        {
            if (x.height > y.height)
                return -1;
            if (x.height == y.height)
                return 0;
            return 1;
        }
    }

    class ImageWidthComparer : IComparer<TP_Image>
    {
        public int Compare(TP_Image x, TP_Image y)
        {
            if (x.width > y.width)
                return -1;
            if (x.width == y.width)
                return 0;
            return 1;
        }
    }

    class ImageAreaComparer : IComparer<TP_Image>
    {
        public int Compare(TP_Image x, TP_Image y)
        {
            int ax = x.width * x.height;
            int ay = y.width * y.height;
            if (ax > ay)
                return -1;
            if (ax == ay)
                return 0;
            return 1;
        }
    }

    void GetExtent(TP_Node r, ref int x, ref int y)
    {
        if (r.img != null) {
            if (r.pixRect.x + r.img.width > x)
                x = r.pixRect.x + r.img.width;

            if (r.pixRect.y + r.img.height > y)
                y = r.pixRect.y + r.img.height;
        }

        if (r.child[0] != null)
            GetExtent(r.child[0], ref x, ref y);
        if (r.child[1] != null)
            GetExtent(r.child[1], ref x, ref y);
    }

    bool Probe(TP_Image[] imgsToAdd, int idealAtlasW, int idealAtlasH, float imgArea, int maxAtlasDimension, TP_ProbeResult pr)
    {
        TP_Node root = new TP_Node();
        root.pixRect = new TP_PixRect(0, 0, idealAtlasW, idealAtlasH);
        for (int i = 0; i < imgsToAdd.Length; i++)
        {
            TP_Node n = root.Insert(imgsToAdd[i], false);
            if (n == null) {
                return false;
            }
            else if (i == imgsToAdd.Length - 1) {
                int usedW = 0;
                int usedH = 0;

                GetExtent(root, ref usedW, ref usedH);

                float efficiency = 1f - (usedW * usedH - imgArea) / (usedW * usedH);

                float squareness;
                if (usedW < usedH) 
                    squareness = (float)usedW / (float)usedH;
                else 
                    squareness = (float)usedH / (float)usedW;

                bool fitsInMaxDim = usedW <= maxAtlasDimension && usedH <= maxAtlasDimension;

                pr.Set(usedW, usedH, root, fitsInMaxDim, efficiency, squareness);

                return true;
            }
        }
        return false;
    }

    public int atlasCount = 0;

    public TP_Image[] Pack(List<Vector2> imgWidthHeights, int maxDimension, int padding)
    {
        atlasCount = 1;
        List<TP_Image> pImgs = new List<TP_Image>();
        float area = 0;
        int maxW = 0;
        int maxH = 0;
        TP_Image[] imgsToAdd = new TP_Image[imgWidthHeights.Count];
        int maxArea = (int)(maxDimension * maxDimension * 0.9);
        int atlasIndex = 0;
        for (int i = 0; i < imgsToAdd.Length; i++)
        {
            TP_Image im = imgsToAdd[i] = new TP_Image(i, (int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, padding);
            maxW = Mathf.Max(maxW, im.width);
            maxH = Mathf.Max(maxH, im.height);
        }

        //if ((float)maxH / (float)maxW > 2)
            //Array.Sort(imgsToAdd, new ImageHeightComparer());
        //else if ((float)maxH / (float)maxW < .5)
            //Array.Sort(imgsToAdd, new ImageWidthComparer());
        //else
            Array.Sort(imgsToAdd, new ImageAreaComparer());

        List<TP_Image> curImgs = new List<TP_Image>();

        TP_Node root = new TP_Node();
        root.pixRect = new TP_PixRect(0, 0, maxDimension, maxDimension);
  
        for (int i = 0; i < imgsToAdd.Length; i++)
        {
            TP_Image im = imgsToAdd[i];
            if (im != null)
            {
                TP_Node n = root.Insert(imgsToAdd[i], false);
                if (n == null)
                {
                    var isMatching = false;
                    for (int j = i + 1; j < imgsToAdd.Length; j++)
                    {
                        TP_Image imj = imgsToAdd[j];
                        if (imj != null)
                        {
                            TP_Node nj = root.Insert(imgsToAdd[j], false);
                            if (nj != null)
                            {
                                imgsToAdd[j] = null;
                                isMatching = true;
                                im = imj;
                                break;
                            }
                        }
                    }

                    if (isMatching == false)
                    {
                        root = new TP_Node();
                        root.pixRect = new TP_PixRect(0, 0, maxDimension, maxDimension);
                        root.Insert(imgsToAdd[i], false);

                        atlasIndex++;
                        atlasCount++;
                        int w = 0;
                        int h = 0;
                        TP_Image[] imgs = GetRects(curImgs.ToArray(), maxDimension, padding, out w, out h);
                        pImgs.AddRange(imgs);
                        curImgs.Clear();
                    }
                    else
                    {
                        --i;
                    }
                }
                im.atlasIndex = atlasIndex;
                curImgs.Add(im);
            }
        }
            
        {
            int w = 0;
            int h = 0;
            TP_Image[] imgs = GetRects(curImgs.ToArray(), maxDimension, padding, out w, out h);
            pImgs.AddRange(imgs);
        }

        return pImgs.ToArray();
    }

    public TP_Image[] GetRects(TP_Image[] imgsToAdd, int maxDimension, int padding, out int outW, out int outH)
    {
        TP_ProbeResult bestRoot = null;
        float area = 0;
        int maxW = 0;
        int maxH = 0;

        for (int i = 0; i < imgsToAdd.Length; i++)
        {
            TP_Image im = imgsToAdd[i];
            area += im.width * im.height;
            maxW = Mathf.Max(maxW, im.width);
            maxH = Mathf.Max(maxH, im.height);
        }

        int sqrtArea = (int)Mathf.Sqrt(area);
        int idealAtlasW = sqrtArea;
        int idealAtlasH = sqrtArea;
        if (maxW > sqrtArea)
        {
            idealAtlasW = maxW;
            idealAtlasH = Mathf.Max(Mathf.CeilToInt(area / maxW), maxH);
        }
        if (maxH > sqrtArea)
        {
            idealAtlasW = Mathf.Max(Mathf.CeilToInt(area / maxH), maxW);
            idealAtlasH = maxH;
        }
        if (idealAtlasW == 0) idealAtlasW = 1;
        if (idealAtlasH == 0) idealAtlasH = 1;

        int stepW = (int)(idealAtlasW * .15f);
        int stepH = (int)(idealAtlasH * .15f);

        if (stepW == 0) stepW = 1;
        if (stepH == 0) stepH = 1;

        TP_ProbeResult pr = new TP_ProbeResult();
        if (Probe(imgsToAdd, maxDimension, maxDimension, area, maxDimension, pr))
            bestRoot = pr;

        outW = 0;
        outH = 0;
        if (bestRoot == null) 
            return null;

        outW = bestRoot.width;
        outH = bestRoot.height;

        List<TP_Image> images = new List<TP_Image>();
        flattenTree(bestRoot.root, images);
        images.Sort(new ImgIDComparer());

        if (images.Count != imgsToAdd.Length)
            Debug.LogError("Result images not the same lentgh as source");

        return images.ToArray();
    }

    static void printTree(TP_Node r, string spc)
    {
        if (r.child[0] != null)
            printTree(r.child[0], spc + "  ");
        if (r.child[1] != null)
            printTree(r.child[1], spc + "  ");
    }

    static void flattenTree(TP_Node r, List<TP_Image> putHere)
    {
        if (r.img != null)
        {
            r.img.x = r.pixRect.x;
            r.img.y = r.pixRect.y;
            putHere.Add(r.img);
        }
        if (r.child[0] != null)
            flattenTree(r.child[0], putHere);
        if (r.child[1] != null)
            flattenTree(r.child[1], putHere);
    }

}
