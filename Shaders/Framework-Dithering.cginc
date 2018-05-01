#ifndef DITHERING_INCLUDED
#define DITHERING_INCLUDED

inline float4 GetDitherPos(float4 vertex, float ditherSize) {
	// Get the dither pixel position from the screen coordinates.
	float3 screenPos = mul(unity_WorldToCamera , mul(unity_ObjectToWorld, vertex));
	return float4(screenPos.xy * _ScreenParams.xy / ditherSize, 0, 1);
}

inline float4 GetDitherValue(sampler2D ditherTex, float4 ditherPos) {
    return tex2D(ditherTex, ditherPos.xy / ditherPos.w).r;
}

inline float3 GetDitherColor(float3 color, sampler2D ditherTex, sampler2D paletteTex,
							 float paletteHeight, float4 ditherPos, float colorCount) {
	// To find the palette color to use for this pixel:
	//	The row offset decides which row of color squares to use.
	//	The red component decides which column of color squares to use.
	//	The green and blue components points to the color in the 16x16 pixel square.
	float ditherValue = tex2D(ditherTex, ditherPos.xy / ditherPos.w).r;
	float2 paletteUV = float2(
		min(floor(color.r * 16), 15) / 16 + clamp(color.b * 16, 0.5, 15.5) / 256,
		(clamp(color.g * 16, 0.5, 15.5) + floor(ditherValue * colorCount) * 16) / paletteHeight);

	// Return the new color from the palette texture
	return tex2D(paletteTex, paletteUV).rgb;
}

inline float3 GetDitherColorSimple(float3 color, sampler2D paletteTex, float paletteHeight,
								   float4 screenPos, float colorCount) {
	// A simplified version of the GetDitherColor function which uses
	// a fixed 4 color matrix and a 1:1 pixel size.
	screenPos.xy = floor((screenPos.xy / screenPos.w) *_ScreenParams.xy) + 0.01;
	float rowOffset = floor((fmod(screenPos.x, 2) * 0.249 +
		fmod(screenPos.x + screenPos.y, 2) * 0.499) * colorCount) * 16;

	float2 paletteUV = float2(
		clamp(floor(color.r * 16), 0, 15) / 16 + clamp(color.b * 16, 0.5, 15.5) / 256,
		(clamp(color.g * 16, 0.5, 15.5) + rowOffset) / paletteHeight);

	return tex2D(paletteTex, paletteUV).rgb;
}

#endif