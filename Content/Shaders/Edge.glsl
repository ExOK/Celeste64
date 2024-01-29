VERTEX:
#version 330

uniform mat4 u_matrix;
layout(location=0) in vec2 a_position;
layout(location=1) in vec2 a_tex;
layout(location=2) in vec4 a_color;
layout(location=3) in vec4 a_type;
out vec2 v_tex;
out vec4 v_col;
out vec4 v_type;
void main(void)
{
	gl_Position = u_matrix * vec4(a_position.xy, 0, 1);
	v_tex = a_tex;
	v_col = a_color;
	v_type = a_type;
}

FRAGMENT:
#version 330
#include Partials/Methods.gl

uniform sampler2D u_texture;
uniform sampler2D u_depth;
uniform vec2 u_pixel;
uniform vec4 u_edge;
in vec2 v_tex;
in vec4 v_col;
in vec4 v_type;
out vec4 o_color;

float depth(vec2 uv)
{
	return texture(u_depth, uv).r;
}

void main(void)
{
	// get depth and adjacent depth values
	float it = depth(v_tex);
	float other = 
		depth(v_tex + vec2(u_pixel.x, 0)) * 0.25 +
		depth(v_tex + vec2(-u_pixel.x, 0)) * 0.25 +
		depth(v_tex + vec2(0, u_pixel.y)) * 0.25 +
		depth(v_tex + vec2(0, -u_pixel.y)) * 0.25;
	
	// more edge the closer to the screen
	float edge = step(0.001, other - it);

	// calculate edge color mixed with default color
	vec4 col = texture(u_texture, v_tex);
	vec3 res = mix(col.rgb, u_edge.rgb, edge * 0.70);
	o_color = vec4(res, col.a);
}
        