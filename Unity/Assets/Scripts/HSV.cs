using UnityEngine;
using System.Collections;

public static class HSV {
	public static void SetHue( ref Color c, float h ) {
		ColorHSV hsv = rgb2hsv( c );
		hsv.h = h;
		c = hsv2rgb( hsv );
	}
	public static void SetSaturation( ref Color c, float s ) {
		ColorHSV hsv = rgb2hsv( c );
		hsv.s = s;
		c = hsv2rgb( hsv );
	}
	public static void SetValue( ref Color c, float v ) {
		ColorHSV hsv = rgb2hsv( c );
		hsv.v = v;
		c = hsv2rgb( hsv );
	}
	public static void SetHSV( ref Color c, ColorHSV hsv ) {
		c = hsv2rgb( hsv );
	}

	public struct ColorHSV {
		public float h;       // angle in degrees
		public float s;       // percent
		public float v;       // percent
		public float a;
	};
	
	public static ColorHSV rgb2hsv( Color c )
	{
		ColorHSV         ret;
		ret.a = c.a;
		float      min, max, delta;
		
		min = c.r < c.g ? c.r : c.g;
		min = min  < c.b ? min  : c.b;
		
		max = c.r > c.g ? c.r : c.g;
		max = max  > c.b ? max  : c.b;
		
		ret.v = max;                                // v
		delta = max - min;
		if (delta < 0.00001f)
		{
			ret.s = 0;
			ret.h = 0; // undefined, maybe nan?
			return ret;
		}
		if( max > 0.0f ) { // NOTE: if Max is == 0, this divide would cause a crash
			ret.s = (delta / max);                  // s
		} else {
			// if max is 0, then r = g = b = 0              
			// s = 0, v is undefined
			ret.s = 0.0f;
			ret.h = System.Single.NaN;                            // its now undefined
			return ret;
		}
		if( c.r >= max )                           // > is bogus, just keeps compilor happy
			ret.h = ( c.g - c.b ) / delta;        // between yellow & magenta
		else
			if( c.g >= max )
				ret.h = 2.0f + ( c.b - c.r ) / delta;  // between cyan & yellow
		else
			ret.h = 4.0f + ( c.r - c.g ) / delta;  // between magenta & cyan
		
		ret.h *= 60.0f;                              // degrees
		
		if( ret.h < 0.0f )
			ret.h += 360.0f;
		
		return ret;
	}

	public static Color hsv2rgb( ColorHSV c )
	{
		float      hh, p, q, t, ff;
		long        i;
		Color       ret = new Color();
		ret.a = c.a;

		if( c.s <= 0.0f ) {       // < is bogus, just shuts up warnings
			ret.r = c.v;
			ret.g = c.v;
			ret.b = c.v;
			return ret;
		}
		hh = c.h;
		if(hh >= 360.0f) hh = 0.0f;
		hh /= 60.0f;
		i = (long)hh;
		ff = hh - i;
		p = c.v * (1.0f - c.s);
		q = c.v * (1.0f - (c.s * ff));
		t = c.v * (1.0f - (c.s * (1.0f - ff)));
		
		switch(i) {
		case 0:
			ret.r = c.v;
			ret.g = t;
			ret.b = p;
			break;
		case 1:
			ret.r = q;
			ret.g = c.v;
			ret.b = p;
			break;
		case 2:
			ret.r = p;
			ret.g = c.v;
			ret.b = t;
			break;
			
		case 3:
			ret.r = p;
			ret.g = q;
			ret.b = c.v;
			break;
		case 4:
			ret.r = t;
			ret.g = p;
			ret.b = c.v;
			break;
		case 5:
		default:
			ret.r = c.v;
			ret.g = p;
			ret.b = q;
			break;
		}
		return ret;
	}
}
	