﻿namespace PeachPDF.CSS
{
    internal sealed class PerspectiveTransform : ITransform
    {
        private readonly Length _distance;

        internal PerspectiveTransform(Length distance)
        {
            _distance = distance;
        }

        public TransformMatrix ComputeMatrix()
        {
            return new (
                1f, 
                0f, 
                0f,
                0f,
                1f,
                0f,
                0f,
                0f,
                1f,
                0f,
                0f,
                0f,
                0f,
                0f,
                -1f/_distance.ToPixel());
        }
    }
}