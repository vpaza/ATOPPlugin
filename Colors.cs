using vatsys.Plugin;

namespace ATOP
{
    internal class Colors
    {
        internal readonly static CustomColour White = new CustomColour(240, 255, 255);
        internal readonly static CustomColour Yellow = new CustomColour(236, 201, 8);
        internal readonly static CustomColour Tangerine = new CustomColour(242, 133, 0);
        internal readonly static CustomColour DarkMagenta = new CustomColour(100, 0, 100);
        internal readonly static CustomColour Magenta = new CustomColour(175, 0, 175);
        internal readonly static CustomColour LightBlue = new CustomColour(0, 196, 253);
        internal readonly static CustomColour Green = new CustomColour(46, 139, 87);

        internal readonly static CustomColour EastboundTracks = White;
        internal readonly static CustomColour WestboundTracks = Yellow;
        internal readonly static CustomColour NotReducedVerticalSeparationMinima = Tangerine;
        internal readonly static CustomColour NotCurrentDataAuthority = Magenta;
        internal readonly static CustomColour SeparationFlags = LightBlue;
        internal readonly static CustomColour Pending = Green;
        internal readonly static CustomColour HighlightFieldStrip = LightBlue;
        internal readonly static CustomColour RadarStrip = Yellow;
    }
}
