﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MapEditor.Classes
{
    public class Map
    {
        public List<SegmentDefinition> SegmentDefinitions { set; get; }

        // Up to three layers, 64 segments
        public MapSegment[,] Segments { get; set; }
        public Dictionary<int, List<MapSegment> > MapSegments;

        public Map()
        {
            Segments = new MapSegment[3, 64];
            MapSegments = new Dictionary<int, List<MapSegment>>();
            SegmentDefinitions = new List<SegmentDefinition>();

            ReadSegmentDefinitions();
        }

        private void ReadSegmentDefinitions()
        {
            StreamReader sr = new StreamReader(@"Content/maps.zdx");
            
            string line = "";

            int n;
            int currentTexture = 0;
            int currentDefinition = -1;
            Rectangle rect = new Rectangle();
            string[] split;

            // Skip the first line
            line = sr.ReadLine();

            // #src n
            // name
            // left top right bottom
            // flags
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                // #src n
                if (line.StartsWith("#"))
                {
                    if (line.StartsWith("#src"))
                    {
                        split = line.Split(' ');

                        if (split.Length > 1)
                        {
                            n = Convert.ToInt32(split[1]);
                            currentTexture = n - 1;
                        }
                    }
                }
                else
                {
                    currentDefinition++;

                    string name = line;

                    line = sr.ReadLine();

                    split = line.Split(' ');

                    if (split.Length > 3)
                    {
                        rect.X = Convert.ToInt32(split[0]);
                        rect.Y = Convert.ToInt32(split[1]);
                        rect.Width = Convert.ToInt32(split[2]) - rect.X;
                        rect.Height = Convert.ToInt32(split[3]) - rect.Y;
                    }
                    else
                    {
                        Console.WriteLine("Read failed: " + name);
                    }

                    int texture = currentTexture;

                    line = sr.ReadLine();
                    int flags = Convert.ToInt32(line);

                    SegmentDefinitions.Add(new SegmentDefinition(name, texture, rect, flags));
                }
            }
        }

        public int AddSegment(int layer, int index)
        {
            for (int i = 0; i < 64; i++)
            {
                if (Segments[layer, i] == null)
                {
                    Segments[layer, i] = new MapSegment();
                    Segments[layer, i].Index = index;
                }
            }

            if (!MapSegments.ContainsKey(layer))
            {
                MapSegments[layer] = new List<MapSegment>();
            }

            var mapSegment = new MapSegment();
            mapSegment.Index = index;
            MapSegments[layer].Add(mapSegment);

            return MapSegments[layer].Count - 1;

            //return -1;
        }

        public void Draw(SpriteBatch sprite, Texture2D[] mapTexture, Vector2 scroll)
        {
            Rectangle sourceRect = new Rectangle();
            Rectangle destRect = new Rectangle();

            sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            foreach (int key in MapSegments.Keys)
            {
                float scale = 1.0f;
                float sizeScale = 1.0f;

                Color color = Color.White;

                if (key == 0)
                {
                    color = Color.Gray;
                    //scale = 0.75f;
                    sizeScale = 0.75f;
                }
                else if (key == 2)
                {
                    color = Color.DarkGray;
                    //scale = 1.25f;
                    sizeScale = 1.25f;
                }

                scale *= 0.5f;
                sizeScale *= 0.5f;

                for (int index = 0; index < MapSegments[key].Count; index++)
                {
                    sourceRect = SegmentDefinitions[MapSegments[key][index].Index].SourceRect;

                    destRect.X = (int)(MapSegments[key][index].Location.X - scroll.X * scale);
                    destRect.Y = (int)(MapSegments[key][index].Location.Y - scroll.Y * scale);

                    destRect.Width = (int)(sourceRect.Width * sizeScale);
                    destRect.Height = (int)(sourceRect.Height * sizeScale);

                    sprite.Draw(mapTexture[SegmentDefinitions[MapSegments[key][index].Index].SourceIndex], destRect, sourceRect, color);
                }
            }

            sprite.End();
        }

        public int GetHoveredSegment(int x, int y, int currentLayer, Vector2 scroll)
        {
            float scale = 1.0f;

            if (currentLayer == 0)
            {
                scale = 0.75f;
            }
            else if (currentLayer == 2)
            {
                scale = 1.25f;
            }

            scale *= 0.5f;

            if (!MapSegments.ContainsKey(currentLayer))
            {
                return -1;
            }

            foreach (int key in MapSegments.Keys)
            {
                for (int index = 0; index < MapSegments[key].Count; index++)
                {
                        Rectangle sourceRect = SegmentDefinitions[MapSegments[currentLayer][index].Index].SourceRect;

                        Rectangle destRect = new Rectangle((int)(MapSegments[currentLayer][index].Location.X - scroll.X * scale),
                                                           (int)(MapSegments[currentLayer][index].Location.Y - scroll.Y * scale),
                                                           (int)(sourceRect.Width * scale),
                                                           (int)(sourceRect.Height * scale));

                        if (destRect.Contains(x, y))
                        {
                            return index;
                        }
                   }
            }

            return -1;
        }
    }
}
