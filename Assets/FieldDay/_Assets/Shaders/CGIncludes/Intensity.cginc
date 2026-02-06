#ifndef FD_INTENSITY_INCLUDED
#define FD_INTENSITY_INCLUDED

#include "./Common.cginc"

/// Configuration Defines

// FD_INTENSITY_COLOR          Multiplies color by texture intensity
// FD_INTENSITY_ALPHA          Multiplies alpha by texture intensity
// FD_INTENSITY_COLOR_ALPHA    Multiplies color and alpha by texture intensity

/// Uniforms

half _IntensityColorThreshold;
half _IntensityAlphaThreshold;

/// Helpers

inline float4 LayerIntensityTexture(sampler2D intensityTexture, float2 uv, float4 color)
{
    float intensity = SampleSingle(intensityTexture, uv);
    return float4(
#if FD_INTENSITY_COLOR || FD_INTENSITY_COLOR_ALPHA
        color.rgb * saturate(intensity / _IntensityColorThreshold),
#else
        color.rgb,
#endif // FD_INTENSITY_COLOR || FD_INTENSITY_COLOR_ALPHA
#if FD_INTENSITY_ALPHA || FD_INTENSITY_COLOR_ALPHA
        saturate(intensity / _IntensityAlphaThreshold) * color.a
#else
        color.a
#endif // FD_INTENSITY_ALPHA || FD_INTENSITY_COLOR_ALPHA
    );
}

#endif // FD_INTENSITY_INCLUDED