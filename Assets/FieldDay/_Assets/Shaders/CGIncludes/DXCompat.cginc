#ifndef FD_DXCOMPAT_INCLUDED
#define FD_DXCOMPAT_INCLUDED

#include "UnityCG.cginc"

/*
#if !defined(sampler2DArray)

struct sampler2DArray                           { Texture2DArray t; SamplerState s; };
float4 tex2DArray(sampler2DArray x, float3 v)   { return x.t.Sample(x.s, v); }

#endif // !defined(sampler2DArray)
*/

#endif // FD_DXCOMPAT_INCLUDED