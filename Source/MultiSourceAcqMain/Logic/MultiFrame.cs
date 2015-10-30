// MultiSourceAcquisition (MSA)
// Copyright (c) 2015 Filip Malawski
// AGH University of Science and Technology, Cracow, Poland
// Contact: fmal@agh.edu.pl
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace MultiSourceAcqMain.Logic
{
    public class MultiFrame
    {
        public long FrameNb { get; set; }

        // input
        public ushort[] DepthData { get; set; }
        public byte[] ColorData { get; set; }
        public byte[] BodyIndexData { get; set; }
        public Body[] Bodies { get; set; }
        public byte[] PS3EyeData { get; set; }
        //public DepthSpacePoint[] ColorMappedToDepthPoints { get; set; }       // not necessary to store
        //public ColorSpacePoint[] DepthMappedToColorPoints { get; set; }       // not necessary to store
        public bool HasKinectData { get; set; }
        public bool HasPS3EyeData { get; set; }

        // tmp
        public byte[] ColorResizedData { get; set; }
        public Bitmap ColorResizedBitmap { get; set; }

        // processed
        public Bitmap ColorBitmap { get; set; }
        public Bitmap DepthBitmap { get; set; }
        public byte[] DepthBytes { get; set; }          // underlying data for bitmap (includes bodyindexdata)
        public Bitmap PS3EyeBitmap { get; set; }
        public float[] BodyData { get; set; }

        // mapped
        public Bitmap ColorMappedBitmap { get; set; }
        public byte[] ColorMappedBytes { get; set; }    // underlying data for bitmap 
        public Bitmap DepthMappedBitmap { get; set; }
        public byte[] DepthMappedBytes { get; set; }    // underlying data for bitmap 

        // flags
        public bool IsLastRecorded { get; set; }

        public void Dispose()
        {
            DepthData = null;
            ColorData = null;
            BodyIndexData = null;
            Bodies = null;

            ColorResizedData = null;
            ColorResizedBitmap = null;

            ColorBitmap = null;
            DepthBitmap = null;
            DepthBytes = null;

            ColorMappedBitmap = null;
            ColorMappedBytes = null;
            DepthMappedBitmap = null;
            DepthMappedBytes = null;
        }
    }
}
