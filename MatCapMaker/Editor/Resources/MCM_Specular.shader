Shader "Hidden/MCM_Specular"
{
    Properties
    {
        _Smoothness ("Smoothness", Float) = 0.8
        _Intensity ("Intensity", Float) = 1
        _ReflectionColor ("Reflection Color", Color) = (1, 1, 1, 1)
        _FresnelBias ("Fresnel Bias", Float) = 0.1
        _FresnelPower ("Fresnel Power", Float) = 3
        _HorizonColor ("Horizon Color", Color) = (0.5, 0.5, 0.6, 1)
        _GroundColor ("Ground Color", Color) = (0.2, 0.15, 0.1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _StudioBaseTex;
            float _StudioTextureStrength;
            float _StudioEnabled;
            float4 _MetalColor, _ShadowColor, _Surface, _Studio, _Pattern, _PatternColor;
            float _PatternSeed, _PatternWrap;
            float4 _PatternControl;
            float4 _Panel0, _Panel1, _Panel2;
            float4 _PanelSize0, _PanelSize1, _PanelSize2;
            float4 _PanelColor0, _PanelColor1, _PanelColor2;
            float _Smoothness;
            float _Intensity;
            float4 _ReflectionColor;
            float _FresnelBias;
            float _FresnelPower;
            float4 _HorizonColor;
            float4 _GroundColor;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float3 getSphereNormal(float2 uv)
            {
                float2 centered = uv * 2.0 - 1.0;
                float r2 = dot(centered, centered);

                if (r2 > 1.0)
                    return float3(0, 0, 0);

                float z = sqrt(1.0 - r2);
                return normalize(float3(centered.x, centered.y, z));
            }

            float3 sampleProceduralEnvironment(float3 reflDir, float smoothness)
            {
                float skyGround = reflDir.y * 0.5 + 0.5;  

                float sharpness = lerp(0.2, 2.0, smoothness);
                skyGround = saturate((skyGround - 0.5) * sharpness + 0.5);

                float3 groundCol = _GroundColor.rgb;
                float3 horizonCol = _HorizonColor.rgb;

                float horizontalAngle = atan2(reflDir.x, reflDir.z) / UNITY_PI;  
                float horizontalVar = abs(horizontalAngle) * 0.3 * smoothness;

                horizonCol += float3(horizontalVar * 0.1, horizontalVar * 0.05, -horizontalVar * 0.05);

                float3 envColor = lerp(groundCol, horizonCol, skyGround);

                float skyHighlight = pow(saturate(reflDir.y), 2.0 + (1.0 - smoothness) * 6.0);
                envColor += _HorizonColor.rgb * skyHighlight * 0.5 * smoothness;

                float edgeBright = 1.0 - abs(reflDir.z);
                edgeBright = pow(edgeBright, 2.0 - smoothness);
                envColor *= 1.0 + edgeBright * 0.3 * smoothness;

                return envColor;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453);
            }

            float valueNoise(float2 p)
            {
                float2 cell = floor(p), f = frac(p);
                f = f*f*(3-2*f);
                return lerp(lerp(hash(cell),hash(cell+float2(1,0)),f.x),
                    lerp(hash(cell+float2(0,1)),hash(cell+1),f.x),f.y);
            }

            float fractal(float2 p)
            {
                float sum=0, weight=.5, total=0;
                [unroll] for (int octave=0; octave<7; octave++)
                {
                    if (octave < _Pattern.w) { sum += valueNoise(p)*weight; total += weight; }
                    p = float2(p.x*.8-p.y*.6,p.x*.6+p.y*.8)*2.07+17.3;
                    weight *= .5;
                }
                return sum / max(total,.001);
            }

            float surfacePattern(float2 uv)
            {
                float2 p=uv*_Pattern.y+_PatternSeed;
                if (_Pattern.x > 12.5)
                {
                    p += (float2(fractal(p*.35+9),fractal(p*.35+31))-.5)*_PatternControl.x*2;
                    float mineral=fractal(p*1.3);
                    float aggregate=valueNoise(p*4);
                    float flecks=smoothstep(.65,.85,valueNoise(p*7));
                    return saturate(.12+mineral*.55+aggregate*.23+flecks*.1);
                }
                if (_Pattern.x > 5.5)
                {
                    p += (float2(fractal(p+9),fractal(p+31))-.5)*_PatternControl.x*3;
                    if (_Pattern.x < 6.5)
                    {
                        float2 domain=_PatternWrap > .5 ? float2((uv.x-.75)*2,uv.y-.5) : uv-.5;
                        float2 z=domain*_Pattern.y;
                        z += (float2(fractal(z+_PatternSeed+9),fractal(z+_PatternSeed+31))-.5)*_PatternControl.x*1.5;
                        float2 c=float2(-.745,.186)+sin(_PatternSeed)*.06;
                        int iterationLimit=16+(int)clamp(_Pattern.w,2,7)*8;
                        float orbit=10, iterations=0;
                        [loop] for (int j=0;j<72;j++)
                        {
                            if(j>=iterationLimit) break;
                            z=float2(z.x*z.x-z.y*z.y,2*z.x*z.y)+c;
                            orbit=min(orbit,abs(length(z)-.7));
                            iterations=j;
                            if(dot(z,z)>64) break;
                        }
                        return saturate(exp(-orbit*18)*.8+sqrt(iterations/iterationLimit)*.2);
                    }
                    if (_Pattern.x < 7.5)
                    {
                        float2 center=p/_Pattern.y-.5;
                        float phase=atan2(center.y,center.x)*5-length(center)*32+fractal(p)*5;
                        return pow(.5+.5*sin(phase),3);
                    }
                    if (_Pattern.x > 8.5 && _Pattern.x < 10.5)
                    {
                        float2 tile=p*float2(1.1547005,1);
                        tile.x+=fmod(floor(tile.y),2)*.5;
                        float2 q=frac(tile)-.5;
                        if (_Pattern.x < 9.5) return 1-smoothstep(.38,.49,max(abs(q.x),abs(q.y)*.866+abs(q.x)*.5));
                        return pow(saturate(1-length(q*float2(1.6,1.1))),.45);
                    }
                    if (_Pattern.x > 11.5)
                    {
                        float flower=0;
                        [unroll] for (int fy=-1;fy<=1;fy++) [unroll] for (int fx=-1;fx<=1;fx++)
                        {
                            float2 cell=floor(p)+float2(fx,fy);
                            float seed=hash(cell);
                            float2 center=cell+.5+(float2(seed,hash(cell+31))-.5)*.45;
                            float2 q=p-center;
                            float angle=atan2(q.y,q.x)+seed*6.283;
                            float radius=length(q);
                            float petals=.42+.16*cos(angle*5);
                            float edge=1-smoothstep(petals-.03,petals+.02,radius);
                            float veins=.97+.03*cos(angle*15+radius*8);
                            float sheen=.45+.55*pow(saturate(radius/petals),.6);
                            flower=max(flower,edge*veins*sheen*(.65+seed*.35));
                        }
                        return flower;
                    }
                    if (_Pattern.x > 10.5)
                    {
                        float shards=0;
                        [unroll] for (int sy=-1;sy<=1;sy++) [unroll] for (int sx=-1;sx<=1;sx++)
                        {
                            float2 tile=floor(p)+float2(sx,sy);
                            float seed=hash(tile);
                            float2 q=p-tile-.5-(float2(seed,hash(tile+19))-.5)*.5;
                            float angle=seed*6.2831853;
                            q=float2(q.x*cos(angle)-q.y*sin(angle),q.x*sin(angle)+q.y*cos(angle));
                            float halfWidth=.26*saturate((.8-q.y)/1.6);
                            float distance=max(abs(q.x)-halfWidth,abs(q.y)-.8);
                            float edge=1-smoothstep(-.012,.012,distance);
                            float facet=lerp(.22+.28*seed,.65+.3*seed,step(0,q.x));
                            float spine=exp(-abs(q.x)*100)*.25;
                            shards=max(shards,edge*saturate(facet+spine+q.y*.12));
                        }
                        return shards;
                    }
                    float2 cell=floor(p), f=frac(p);
                    float nearest=10, second=10, cellShade=0;
                    [unroll] for (int y=-1;y<=1;y++) [unroll] for (int x=-1;x<=1;x++)
                    {
                        float2 offset=float2(x,y);
                        float2 v=offset+float2(hash(cell+offset),hash(cell+offset+39))-f;
                        float d=dot(v,v);
                        if (d<nearest) cellShade=hash(cell+offset+11);
                        second=min(second,max(nearest,d)); nearest=min(nearest,d);
                    }
                    if (_Pattern.x < 8.5) return 1-smoothstep(.02,.12,second-nearest);
                    return cellShade * smoothstep(.005,.035,second-nearest);
                }
                if (_Pattern.x > 4.5)
                {
                    float phase=p.y*24+fractal(p)*4;
                    float visibility=1-smoothstep(1,3,fwidth(phase));
                    return .5+.08*sin(phase)*visibility;
                }
                if (_Pattern.x > 3.5)
                {
                    float2 cell=floor(p), f=frac(p);
                    float nearest=10, second=10, facet=0;
                    [unroll] for (int y=-1;y<=1;y++) [unroll] for (int x=-1;x<=1;x++)
                    {
                        float2 offset=float2(x,y);
                        float2 cellVector=offset+float2(hash(cell+offset),hash(cell+offset+39))-f;
                        float d=dot(cellVector,cellVector);
                        if(d<nearest) facet=hash(cell+offset+11);
                        second=min(second,max(nearest,d)); nearest=min(nearest,d);
                    }
                    float interior=saturate(.15+facet*.45+sqrt(nearest)*.25+fractal(p*1.7)*.3);
                    return interior*smoothstep(.003,.035,second-nearest);
                }
                float2 warp=float2(fractal(p+4.1),fractal(p+27.8));
                float n=fractal(p+warp*3);
                if (_Pattern.x > 2.5) return .5+.5*sin(n*40+p.x*3);
                if (_Pattern.x > 1.5) return pow(1-abs(n*2-1),4);
                return n;
            }

            float panelReflection(float3 direction, float4 panel, float4 shape, float softness)
            {
                float3 axis = normalize(panel.xyz + float3(0,0,0.0001));
                float3 right = normalize(cross(abs(axis.y) > .98 ? float3(0,0,1) : float3(0,1,0), axis));
                float3 up = cross(axis, right);
                float facing = dot(direction, axis);
                float2 panelUV = float2(dot(direction,right), dot(direction,up)) / max(facing,.08);
                float sn = sin(shape.z), cs = cos(shape.z);
                panelUV = float2(panelUV.x * cs - panelUV.y * sn, panelUV.x * sn + panelUV.y * cs);
                float2 halfSize = max(shape.xy * .5, .015);
                float2 q=panelUV/halfSize;
                float d=max(abs(q.x),abs(q.y))-1;
                if (shape.w > 1.5 && shape.w < 2.5) d=length(q)-1;
                if (shape.w > 2.5 && shape.w < 3.5) d=abs(length(q)-.78)-.13;
                if (shape.w > 3.5 && shape.w < 4.5) d=max(abs(q.y),dot(abs(q),float2(.8660254,.5)))-.8660254;
                if (shape.w > 4.5 && shape.w < 5.5) d=length(q)-(.65+.3*cos(atan2(q.y,q.x)*5));
                if (shape.w > 5.5) d=max(length(q)-1,.82-length(q-float2(.4,.15)));
                float coverage = 1 - smoothstep(-softness, softness, d*min(halfSize.x,halfSize.y));
                if (shape.w > .5 && shape.w < 1.5)
                {
                    float barWidth = .018;
                    float bars = smoothstep(barWidth, barWidth + softness * .25, abs(panelUV.x))
                        * smoothstep(barWidth, barWidth + softness * .25, abs(panelUV.y));
                    coverage *= bars;
                }
                return coverage * smoothstep(.05,.25,facing) * panel.w;
            }

            float4 studioReflection(float3 normal, float2 uv)
            {
                float2 patternUV=uv;
                if (_PatternWrap > .5)
                    patternUV=float2(atan2(normal.z,normal.y)/6.2831853+.5,asin(clamp(normal.x,-1,1))/3.14159265+.5);
                float ps=sin(_PatternControl.z), pc=cos(_PatternControl.z);
                float2 patternCenter=_PatternWrap > .5 && _Pattern.x > 5.5 && _Pattern.x < 6.5 ? float2(.75,.5) : float2(.5,.5);
                patternUV-=patternCenter;
                patternUV=float2(patternUV.x*pc-patternUV.y*ps,patternUV.x*ps+patternUV.y*pc)+patternCenter;
                float pattern=.5;
                if (_Pattern.x > .5 && _Pattern.z > 0)
                {
                    pattern=surfacePattern(patternUV);
                    pattern=saturate((pattern-.5)*_PatternControl.w+.5);
                    float2 slope=float2(ddx(pattern),ddy(pattern));
                    float relief=_Pattern.x > 11.5 ? .12 : (_Pattern.x > 4.5 && _Pattern.x < 5.5 ? .08 : .7);
                    normal=normalize(normal+float3(-slope*_Pattern.z*relief,0));
                }
                float3 ripple = float3(sin(normal.y*12 + normal.z*5), sin(normal.x*11-normal.z*7), sin(normal.y*8+normal.x*9));
                normal = normalize(normal + ripple * _Surface.y * .45);
                float3 direction = reflect(float3(0,0,-1),normal);
                float sn=sin(_Studio.x), cs=cos(_Studio.x);
                direction.xy = float2(direction.x*cs-direction.y*sn, direction.x*sn+direction.y*cs);
                float softness = max(.005,_Surface.x);
                float3 highlights = panelReflection(direction,_Panel0,_PanelSize0,softness)*_PanelColor0.rgb;
                highlights += panelReflection(direction,_Panel1,_PanelSize1,softness)*_PanelColor1.rgb;
                highlights += panelReflection(direction,_Panel2,_PanelSize2,softness)*_PanelColor2.rgb;
                float horizon = smoothstep(-.75,.8,direction.y);
                float band = 1 - .7 * exp(-pow((direction.y+.12)*4,2));
                float3 metal = _MetalColor.rgb;
                float phase = (1-normal.z)*5 + direction.y*2.6 + direction.x*1.5;
                float3 rainbow = lerp(float3(.04,.6,.9),float3(.85,.04,.48),.5+.5*sin(phase));
                rainbow = lerp(rainbow,float3(.24,.035,.65),pow(.5+.5*cos(phase*1.4),3)*.65);
                metal = lerp(metal,rainbow,_Surface.w);
                if (_Pattern.x > .5)
                    metal=lerp(metal,lerp(metal*.3,_PatternColor.rgb,pattern),_Pattern.z);
                float3 ambient = lerp(_ShadowColor.rgb,metal*(.045+.32*horizon),band);
                float broad = pow(saturate(dot(direction,normalize(_Panel0.xyz))),3) * _Panel0.w * .15;
                broad += pow(saturate(dot(direction,normalize(_Panel2.xyz))),4) * _Panel2.w * .12;
                float3 tint = lerp(metal, float3(1,1,1), .72 - _Studio.y*.3);
                float pixelWidth=max(length(ddx(uv)),length(ddy(uv)));
                float grainFrequency=min(768,.3/max(pixelWidth,.00001));
                float grain = (valueNoise(uv*grainFrequency)-.5)*_Surface.z*.5;
                float3 color = ambient + (highlights + broad)*tint*(1+grain*4);
                if (_Pattern.x > .5)
                    color += _PatternColor.rgb*pow(saturate(pattern),4)*_Pattern.z*_PatternControl.y;
                color += grain * (.35 + highlights*.3);
                float edge=pow(1-saturate(normal.z),12);
                color += metal*edge*(.35+_Studio.y*.5);
                if (_Pattern.x > 12.5)
                {
                    float diffuse=.35+.65*saturate(dot(normal,normalize(_Panel0.xyz)));
                    float3 albedo=lerp(_MetalColor.rgb,lerp(_MetalColor.rgb*.55,_PatternColor.rgb,pattern),_Pattern.z);
                    color=albedo*diffuse+grain*.1;
                    color+=_PatternColor.rgb*pow(pattern,4)*_Pattern.z*_PatternControl.y;
                }
                color *= lerp(float3(1,1,1), tex2D(_StudioBaseTex,uv).rgb, _StudioTextureStrength);
                color = max(0,color);
                color = color / (1 + color * .5);
                return float4(color,1);
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 normal = getSphereNormal(i.uv);

                if (length(normal) < 0.001)
                    return fixed4(0, 0, 0, 0);

                if (_StudioEnabled > .5) return studioReflection(normal, i.uv);

                float3 viewDir = float3(0, 0, 1);

                float3 reflDir = reflect(-viewDir, normal);

                float NdotV = max(dot(normal, viewDir), 0.0);
                float fresnel = _FresnelBias + (1.0 - _FresnelBias) * pow(1.0 - NdotV, _FresnelPower);

                float3 envReflection = sampleProceduralEnvironment(reflDir, _Smoothness);

                float3 result = envReflection * fresnel * _Intensity * _ReflectionColor.rgb;

                float alpha = saturate(max(max(result.r, result.g), result.b));

                return fixed4(result, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}

