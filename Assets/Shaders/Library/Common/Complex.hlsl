float2 cadd( float2 a, float s ) { return float2( a.x+s, a.y ); }
float2 cmul( float2 a, float2 b ) { return float2( a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x ); }
float2 cdiv( float2 a, float2 b ) { float d = dot(b,b); return float2( dot(a,b), a.y*b.x - a.x*b.y ) / d; }
float2 cpow( float2 z, float n ) { float r = length( z ); float a = atan2( z.y, z.x ); return pow( r, n )*float2( cos(a*n), sin(a*n) ); }
float2 csqrt( float2 z ) { float m = length(z); return sqrt( 0.5*float2(m+z.x, m-z.x) ) * float2( 1.0, sign(z.y) ); }
float2 cconj( float2 z ) { return float2(z.x,-z.y); }
