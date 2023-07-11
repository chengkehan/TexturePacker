using System;
using System.Collections.Generic;

public class TP_ProbeResult
{
    public int width;                               
    public int height;                              
    public TP_Node root;                            
    public bool fitsInMaxSize;                      
    public float efficiency;                        
    public float squareness;                        

    public void Set(int width, int height, TP_Node root, bool fits, float efficiency, float squareness)
    {
        this.width = width;
        this.height = height;
        this.root = root;
        this.fitsInMaxSize = fits;
        this.efficiency = efficiency;
        this.squareness = squareness;
    }

    public float GetScore()
    {
        float fitsScore = fitsInMaxSize ? 1f : 0f;
        return squareness + 2 * efficiency + fitsScore;
    }
}
