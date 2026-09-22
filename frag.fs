#ifdef GL_ES
#extension GL_OES_standard_derivatives : enable
precision highp float;
#endif

uniform vec2 u_mouse;
uniform vec2 u_resolution;
uniform float u_time;

float time;

mat3 rotX(float a)
{
    float c = cos(a);
    float s = sin(a);

    return mat3(
        1, 0, 0,
        0, c, -s,
        0, s, c
    );
}

mat3 rotY(float a)
{
    float c = cos(a);
    float s = sin(a);

    return mat3(
         c, 0, s,
         0, 1, 0,
        -s, 0, c
    );
}

mat3 rotZ(float a)
{
    float c = cos(a);
    float s = sin(a);

    return mat3(
        c, -s, 0,
        s,  c, 0,
        0,  0, 1
    );
}

float sdCircle(vec3 p, float r){
    return length(p) - r;
}

float sdBox(vec3 p, vec3 b)
{
    vec3 q = abs(p) - b;
    return length(max(q, 0.0))
         + min(max(q.x, max(q.y, q.z)), 0.0);
}

float opUnion(float d1, float d2)
{
    return min(d1, d2);
}

float sdPlus(vec3 p, float boxSize) {

    p = abs(p); 

    if (p.x > p.y && p.x > p.z) {
        p.x -= boxSize;
    } else if (p.y > p.z) {
        p.y -= boxSize;
    } else {
        p.z -= boxSize;
    }

    p *= 2.0;

    return sdBox(p, vec3(boxSize)) / 2.0; 
}

float sdFractal2(vec3 p, float boxSize) {
    p /= boxSize;
    float fractal = 1000.0;
    for(int z = -1; z <= 1; z++) {
        for(int y = -1; y <= 1; y++) {
            for(int x = -1; x <= 1; x++) {
                if (x * x + y * y + z * z != 1)
                    continue;

                vec3 plusPos = vec3(x,y,z) * 1.2;

                vec3 q = p - plusPos;

                if (x != 0)
                    q *= rotX(time * 0.2);

                if (y != 0)
                    q *= rotY(time * 0.2);

                if (z != 0)
                    q *= rotZ(time * 0.2);

                fractal = min(fractal,sdPlus(q, boxSize * 0.6));
        }
    }
}
    return fractal * boxSize;
}

float sdFractal(vec3 p, float boxSize) {
    p /= boxSize;
    float fractal = 1000.0;
    for(int z = -1; z <= 1; z++) {
        for(int y = -1; y <= 1; y++) {
            for(int x = -1; x <= 1; x++) {
                if (x * x + y * y + z * z != 1)
                    continue;
                vec3 plusPos = vec3(x,y,z) * 1.2;

                vec3 q = p - plusPos;

                if (x != 0)
                    q *= rotX(time * 0.3 * float(x));

                if (y != 0)
                    q *= rotY(time * 0.3 * float(y));

                if (z != 0)
                    q *= rotZ(time * 0.3 * float(z));

                fractal = min(fractal, sdPlus(q, boxSize * 0.6));
                fractal = min(fractal, sdFractal2(q, boxSize * 1.0));
            }
        }
    }
    
    return fractal * boxSize;
}

float sdBoxFrame(vec3 p, vec3 b, float e)
{
    p = abs(p) - b;
    vec3 q = abs(p + e) - e;
    return min(min(
        length(max(vec3(p.x, q.y, q.z), 0.0)) + min(max(p.x, max(q.y, q.z)), 0.0),
        length(max(vec3(q.x, p.y, q.z), 0.0)) + min(max(q.x, max(p.y, q.z)), 0.0)),
        length(max(vec3(q.x, q.y, p.z), 0.0)) + min(max(q.x, max(q.y, p.z)), 0.0));
}

float sdPlusFrame(vec3 p, float boxSize, float e) {
    p = abs(p);

    if (p.x > p.y && p.x > p.z) {
        p.x -= boxSize;
    } else if (p.y > p.z) {
        p.y -= boxSize;
    } else {
        p.z -= boxSize;
    }

    p *= 2.0;

    return sdBoxFrame(p, vec3(boxSize), e * 2.0) / 2.0;
}

float sdFractal2Frame(vec3 p, float boxSize, float e) {
    p /= boxSize;
    float eScaled = e / boxSize;
    float fractal = 1000.0;
    for(int z = -1; z <= 1; z++) {
        for(int y = -1; y <= 1; y++) {
            for(int x = -1; x <= 1; x++) {
                if (x * x + y * y + z * z != 1)
                    continue;

                vec3 plusPos = vec3(x,y,z) * 1.2;
                vec3 q = p - plusPos;

                if (x != 0) q *= rotX(time * 0.2);
                if (y != 0) q *= rotY(time * 0.2);
                if (z != 0) q *= rotZ(time * 0.2);

                fractal = min(fractal, sdPlusFrame(q, boxSize * 0.6, eScaled));
        }
    }
}
    return fractal * boxSize;
}

float sdFractalFrame(vec3 p, float boxSize, float e) {
    p /= boxSize;
    float eScaled = e / boxSize;
    float fractal = 1000.0;
    for(int z = -1; z <= 1; z++) {
        for(int y = -1; y <= 1; y++) {
            for(int x = -1; x <= 1; x++) {
                if (x * x + y * y + z * z != 1)
                    continue;
                vec3 plusPos = vec3(x,y,z) * 1.2;
                vec3 q = p - plusPos;

                if (x != 0) q *= rotX(time * 0.3 * float(x));
                if (y != 0) q *= rotY(time * 0.3 * float(y));
                if (z != 0) q *= rotZ(time * 0.3 * float(z));

                fractal = min(fractal, sdPlusFrame(q, boxSize * 0.6, eScaled));
                fractal = min(fractal, sdFractal2Frame(q, boxSize * 1.0, eScaled));
            }
        }
    }
    return fractal * boxSize;
}

float edgeMap(vec3 p, float e) {
    p *= rotY(time * 0.712);
    float size = 0.5;
    float plus = sdPlusFrame(p, size * 0.5, e);
    return min(plus, sdFractalFrame(p, size, e));
}


float map(vec3 p) {
    p *= rotY(time * 0.712);

    float size = 0.5;

    float plus = sdPlus(p, size * 0.5);

    return min(plus,sdFractal(p,size));
}

vec3 getNormal(vec3 p)
{
    const float e = 0.001;

    return normalize(vec3(
        map(p + vec3(e, 0.0, 0.0)) - map(p - vec3(e, 0.0, 0.0)),
        map(p + vec3(0.0, e, 0.0)) - map(p - vec3(0.0, e, 0.0)),
        map(p + vec3(0.0, 0.0, e)) - map(p - vec3(0.0, 0.0, e))
    ));
}

float edgeCurvature(vec3 p, vec3 n) {
    float e = 0.01;
    float d0 = map(p);
    float d1 = map(p + n * e);
    float d2 = map(p - n * e);
    return abs(d1 + d2 - 2.0 * d0) / (e * e);
}

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float hash3(vec3 p) {
    return fract(sin(dot(p, vec3(127.1, 311.7, 74.7))) * 43758.5453123);
}

float noise3(vec3 p) {
    vec3 i = floor(p);
    vec3 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(
        mix(mix(hash3(i+vec3(0,0,0)), hash3(i+vec3(1,0,0)), f.x),
            mix(hash3(i+vec3(0,1,0)), hash3(i+vec3(1,1,0)), f.x), f.y),
        mix(mix(hash3(i+vec3(0,0,1)), hash3(i+vec3(1,0,1)), f.x),
            mix(hash3(i+vec3(0,1,1)), hash3(i+vec3(1,1,1)), f.x), f.y),
        f.z);
}

float fbm3(vec3 p) {
    float v = 0.0;
    float amp = 0.5;
    for(int i = 0; i < 4; i++) {
        v += amp * noise3(p);
        p *= 2.02;
        amp *= 0.5;
    }
    return v;
}

float hatchStroke(vec2 fragCoord, float angleDeg, float freq, float seedOffset) {
    float a = radians(angleDeg);
    float ca = cos(a), sa = sin(a);
    vec2 rc = vec2(fragCoord.x * ca - fragCoord.y * sa,
                    fragCoord.x * sa + fragCoord.y * ca);

    float wobble = fbm3(vec3(rc.y * 0.015, seedOffset, 0.0)) * 2.0;
    float spacingJitter = (hash3(vec3(floor(rc.y * freq * 0.05), seedOffset, 1.0)) - 0.5) * 2.5;

    float line = sin((rc.x + wobble + spacingJitter) * freq) * 0.5 + 0.5;
    line = smoothstep(0.42, 0.58, line);

    float breakup = fbm3(vec3(rc * 0.04, seedOffset + 30.0));
    line *= smoothstep(0.25, 0.55, breakup);

    float weight = mix(0.6, 1.0, hash3(vec3(floor(rc.y * freq * 0.05), seedOffset, 2.0)));
    return line * weight;
}

float distToLine(float coord, float spacing) {
    float m = mod(coord, spacing);
    return min(m, spacing - m);
}

void main() {

    float fps = 10.0;
    float stepIndex = floor(u_time * fps);
    time = stepIndex / fps;

    float jitterIndex = floor(stepIndex / 2.482);

    vec2 jitter = vec2(
        hash(vec2(jitterIndex, 1.0)),
        hash(vec2(jitterIndex, 2.0))
    ) - 0.5;

    vec2 uv = gl_FragCoord.xy/u_resolution.xy * 2.0 - 1.0;
    uv += jitter * 0.02;
    float angleJitter = (hash(vec2(jitterIndex, 3.0)) - 0.5) * 0.01;
float ca = cos(angleJitter), sa = sin(angleJitter);
uv = mat2(ca, -sa, sa, ca) * uv;
    vec3 ro = vec3(0.0, 0.5, -5.0);
    vec3 forward = normalize(vec3(0.0) - ro);
    vec3 right = normalize(cross(forward, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(right, forward);
    vec3 rd = normalize(forward * 4.0 + right * uv.x + up * uv.y);

    float t = 0.0;
    bool hitSurface = false;
    for(int i = 0; i < 100; i++) {
        vec3 p = ro + rd * t;
        float d = map(p);
        if(d < 0.001) { hitSurface = true; break; }
        if(t > 1000.0) { break; }
        t += d;
    }

    vec3 paper = vec3(0.98, 0.97, 0.94);
    float paperNoise = fbm3(vec3(gl_FragCoord.xy * 0.08, 0.0));
    paper -= paperNoise * 0.03;
    vec3 ink = vec3(0.12, 0.18, 0.55);

    float contrast = 2.0;
    ink = (ink - 0.5) * contrast + 0.5;

    if(!hitSurface) {
        gl_FragColor = vec4(paper, 1.0);
        return;
    }

    vec3 hitPos = ro + rd * t;
    vec3 normal = getNormal(hitPos);

    vec3 localPos = hitPos * rotY(time * 0.712);

    float warpAmt = 0.0035;
    vec3 warp = vec3(
        fbm3(localPos * 6.0 + 11.0),
        fbm3(localPos * 6.0 + 47.0),
        fbm3(localPos * 6.0 + 93.0)
    ) - 0.5;
    vec3 warpedPos = hitPos + warp * warpAmt;

    float edgeDist = edgeMap(warpedPos, 0.004);
    float internalEdge = 1.0 - smoothstep(0.0, 0.01, edgeDist);

    float depthDeriv = fwidth(t);
    float normalDeriv = length(fwidth(normal));
    float silhouette = smoothstep(0.0, 1.0, depthDeriv * 2.0 + normalDeriv * 1.5);

    float lineMask = max(internalEdge, silhouette);

    float pressure = fbm3(localPos * 30.0 + 5.0);
    lineMask *= mix(0.5, 1.15, smoothstep(0.2, 0.8, pressure));

    float dryPatch = fbm3(localPos * 8.0 + 100.0);
    lineMask *= smoothstep(0.15, 0.5, dryPatch);

    vec3 warp2 = vec3(
        fbm3(localPos * 6.0 + 211.0),
        fbm3(localPos * 6.0 + 247.0),
        fbm3(localPos * 6.0 + 293.0)
    ) - 0.5;
    float edgeDist2 = edgeMap(hitPos + warp2 * (warpAmt * 1.8), 0.004);
    float internalEdge2 = 1.0 - smoothstep(0.0, 0.012, edgeDist2);
    lineMask = max(lineMask, internalEdge2 * 0.4);
    
    vec3 lightDir = normalize(vec3(2.4, 4.6, 0.3));
    float ndotl = dot(normal, lightDir);
    float shadowDepth = smoothstep(0.35, -0.35, ndotl);

    float angleDrift = (hash(vec2(stepIndex, 9.0)) - 0.5) * 3.0;

    float hatch1 = hatchStroke(gl_FragCoord.xy, 45.0 + angleDrift, 0.8, 1.0);
    float layer1 = hatch1 * smoothstep(0.15, 0.5, shadowDepth);

    float hatch = layer1;
    hatch *= 0.85;

    lineMask = max(lineMask, hatch * 0.5);

    float curvature = edgeCurvature(hitPos, normal);
    float blot = smoothstep(20.0, 80.0, curvature);
    lineMask = max(lineMask, blot * 0.6);

    float grain = hash(gl_FragCoord.xy) * 0.15;
    lineMask = clamp(lineMask - grain * (3.0 - lineMask), 0.0, 1.0);

    vec3 color = mix(paper, ink, lineMask);
    gl_FragColor = vec4(color, 1.0);
}