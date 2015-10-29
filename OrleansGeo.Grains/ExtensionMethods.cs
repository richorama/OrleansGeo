using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrleansGeo.GrainInterfaces;

namespace OrleansGeo.Grains
{
    public static class ExtensionMethods
    {

        public static IEnumerable<string> GetAllParents(this string quadKey)
        {
            if (null == quadKey) yield break;

            for (var i = quadKey.Length; i >= 3; i--)
            {
                yield return quadKey.Substring(0, i);
            }
        }

        private const double EarthRadius = 6378137;
        private const double MinLatitude = -85.05112878;
        private const double MaxLatitude = 85.05112878;
        private const double MinLongitude = -180;
        private const double MaxLongitude = 180;
        private const int MaxDepth = 23;


        /// <summary>
        /// Clips a number to the specified minimum and maximum values.
        /// </summary>
        /// <param name="n">The number to clip.</param>
        /// <param name="minValue">Minimum allowable value.</param>
        /// <param name="maxValue">Maximum allowable value.</param>
        /// <returns>The clipped value.</returns>
        private static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }



        /// <summary>
        /// Determines the map width and height (in pixels) at a specified level
        /// of detail.
        /// </summary>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>The map width and height in pixels.</returns>
        public static uint MapSize(int levelOfDetail)
        {
            return (uint)256 << levelOfDetail;
        }



        /// <summary>
        /// Determines the ground resolution (in meters per pixel) at a specified
        /// latitude and level of detail.
        /// </summary>
        /// <param name="latitude">Latitude (in degrees) at which to measure the
        /// ground resolution.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>The ground resolution, in meters per pixel.</returns>
        public static double GroundResolution(double latitude, int levelOfDetail)
        {
            latitude = Clip(latitude, MinLatitude, MaxLatitude);
            return Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * EarthRadius / MapSize(levelOfDetail);
        }



        /// <summary>
        /// Determines the map scale at a specified latitude, level of detail,
        /// and screen resolution.
        /// </summary>
        /// <param name="latitude">Latitude (in degrees) at which to measure the
        /// map scale.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <param name="screenDpi">Resolution of the screen, in dots per inch.</param>
        /// <returns>The map scale, expressed as the denominator N of the ratio 1 : N.</returns>
        public static double MapScale(double latitude, int levelOfDetail, int screenDpi)
        {
            return GroundResolution(latitude, levelOfDetail) * screenDpi / 0.0254;
        }



        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
        /// into pixel XY coordinates at a specified level of detail.
        /// </summary>
        /// <param name="latitude">Latitude of the point, in degrees.</param>
        /// <param name="longitude">Longitude of the point, in degrees.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <param name="pixelX">Output parameter receiving the X coordinate in pixels.</param>
        /// <param name="pixelY">Output parameter receiving the Y coordinate in pixels.</param>
        public static void LatLongToPixelXY(double latitude, double longitude, int levelOfDetail, out int pixelX, out int pixelY)
        {
            latitude = Clip(latitude, MinLatitude, MaxLatitude);
            longitude = Clip(longitude, MinLongitude, MaxLongitude);

            double x = (longitude + 180) / 360;
            double sinLatitude = Math.Sin(latitude * Math.PI / 180);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            uint mapSize = MapSize(levelOfDetail);
            pixelX = (int)Clip(x * mapSize + 0.5, 0, mapSize - 1);
            pixelY = (int)Clip(y * mapSize + 0.5, 0, mapSize - 1);
        }



        /// <summary>
        /// Converts a pixel from pixel XY coordinates at a specified level of detail
        /// into latitude/longitude WGS-84 coordinates (in degrees).
        /// </summary>
        /// <param name="pixelX">X coordinate of the point, in pixels.</param>
        /// <param name="pixelY">Y coordinates of the point, in pixels.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <param name="latitude">Output parameter receiving the latitude in degrees.</param>
        /// <param name="longitude">Output parameter receiving the longitude in degrees.</param>
        public static void PixelXYToLatLong(int pixelX, int pixelY, int levelOfDetail, out double latitude, out double longitude)
        {
            double mapSize = MapSize(levelOfDetail);
            double x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
            double y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);

            latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
            longitude = 360 * x;
        }



        /// <summary>
        /// Converts pixel XY coordinates into tile XY coordinates of the tile containing
        /// the specified pixel.
        /// </summary>
        /// <param name="pixelX">Pixel X coordinate.</param>
        /// <param name="pixelY">Pixel Y coordinate.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        public static void PixelXYToTileXY(int pixelX, int pixelY, out int tileX, out int tileY)
        {
            tileX = pixelX / 256;
            tileY = pixelY / 256;
        }



        /// <summary>
        /// Converts tile XY coordinates into pixel XY coordinates of the upper-left pixel
        /// of the specified tile.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="pixelX">Output parameter receiving the pixel X coordinate.</param>
        /// <param name="pixelY">Output parameter receiving the pixel Y coordinate.</param>
        public static void TileXYToPixelXY(int tileX, int tileY, out int pixelX, out int pixelY)
        {
            pixelX = tileX * 256;
            pixelY = tileY * 256;
        }



        public static string ToQuadKey(this Position position)
        {
            if (null == position) return null;
            if (position.Longitude == 0 && position.Latitude == 0) return null;

            var pixelX = 0;
            var pixelY = 0;
            LatLongToPixelXY(position.Latitude, position.Longitude, MaxDepth, out pixelX, out pixelY);

            var tileX = 0;
            var tileY = 0;
            PixelXYToTileXY(pixelX, pixelY, out tileX, out tileY);

            return TileXYToQuadKey(tileX, tileY, MaxDepth);
        }


        /// <summary>
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>A string containing the QuadKey.</returns>
        public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }



        /// <summary>
        /// Converts a QuadKey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">QuadKey of the tile.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
        public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
        {
            tileX = tileY = 0;
            levelOfDetail = quadKey.Length;
            for (int i = levelOfDetail; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                switch (quadKey[levelOfDetail - i])
                {
                    case '0':
                        break;

                    case '1':
                        tileX |= mask;
                        break;

                    case '2':
                        tileY |= mask;
                        break;

                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;

                    default:
                        throw new ArgumentException("Invalid QuadKey digit sequence.");
                }
            }
        }

        public enum Delta
        {
            Leave,
            Join
        }

        public class Change
        {
            public string QuadKey { get; set; }
            public Delta Delta { get; set; }
        }


        public static IEnumerable<Change> GetDelta(this Position from, Position to)
        {
            if (from != null && from.Latitude == 0 && from.Longitude == 0) from = null;
            if (to != null && to.Latitude == 0 && to.Longitude == 0) to = null;

            return GetDelta(from.ToQuadKey(), to.ToQuadKey());
        }

        public static IEnumerable<Change> GetDelta(string from, string to)
        {
            var fromArray = from.GetAllParents().ToArray();
            var toArray = to.GetAllParents().ToArray();

            foreach (var item in fromArray.Where(x => !toArray.Contains(x)))
            {
                yield return new Change { Delta = Delta.Leave, QuadKey = item };
            }

            foreach (var item in toArray.Where(x => !fromArray.Contains(x)))
            {
                yield return new Change { Delta = Delta.Join, QuadKey = item };
            }
        }


        public static IEnumerable<string> GetQuadKeysInRadius(this Position position, double radius)
        {
            var topleft = position.Add(-radius, -radius);
            var bottomRight = position.Add(radius, radius);

            var topLeftX = 0;
            var topLeftY = 0;
            LatLongToPixelXY(topleft.Latitude, topleft.Longitude, MaxDepth, out topLeftX, out topLeftY);

            var bottomRightX = 0;
            var bottomRightY = 0;
            LatLongToPixelXY(bottomRight.Latitude, bottomRight.Longitude, MaxDepth, out bottomRightX, out bottomRightY);

            // get the pixels
            var pixels = Math.Max(Math.Abs(bottomRightX - topLeftX), Math.Abs(bottomRightY - topLeftY));

            // so we know the number of pixels the diameter of our search is, let's select the right zoom level
            var tiles = pixels / 256;

            var searchLevel = (int)(MaxDepth - Math.Floor(Math.Log(tiles, 2)));
            searchLevel = Math.Max(3, Math.Min(MaxDepth, searchLevel));

            int topLeftTileX = 0;
            int topLeftTileY = 0;
            LatLongToPixelXY(topleft.Latitude, topleft.Longitude, searchLevel, out topLeftTileX, out topLeftTileY);

            int bottomRightTileX = 0;
            int bottomRightTileY = 0;
            LatLongToPixelXY(bottomRight.Latitude, bottomRight.Longitude, searchLevel, out bottomRightTileX, out bottomRightTileY);

            topLeftTileX = topLeftTileX / 256;
            topLeftTileY = topLeftTileY / 256;
            bottomRightTileX = bottomRightTileX / 256;
            bottomRightTileY = bottomRightTileY / 256;

            for (var x = Math.Min(topLeftTileX, bottomRightTileX); x <= Math.Max(topLeftTileX, bottomRightTileX); x++)
            {
                for (var y = Math.Min(topLeftTileY, bottomRightTileY); y <= Math.Max(topLeftTileY, bottomRightTileY); y++)
                {
                    yield return TileXYToQuadKey(x, y, searchLevel);
                }
            }
        }

        public static Position Add(this Position position, double dx, double dy)
        {
            return new Position
            {
                Latitude = position.Latitude + (dy / EarthRadius) * (180 / Math.PI),
                Longitude = position.Longitude + (dx / EarthRadius) * (180 / Math.PI) / Math.Cos(position.Latitude * Math.PI / 180)
            };
        }

        public static double DistanceTo(this Position position1, Position position2)
        {
            var φ1 = DegreesToRadians(position1.Latitude);
            var φ2 = DegreesToRadians(position2.Latitude);
            var Δφ = DegreesToRadians(position2.Latitude - position1.Latitude);
            var Δλ = DegreesToRadians(position2.Longitude - position1.Longitude);

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) + Math.Cos(φ1) * Math.Cos(φ2) * Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        private static double DegreesToRadians(double deg)
        {
            return (deg * Math.PI / 180.0);
        }


    }
}
