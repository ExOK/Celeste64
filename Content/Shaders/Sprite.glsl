VERTEX:
#version 330

uniform mat4 u_matrix;

layout(location=0) in vec3 a_position;
layout(location=1) in vec2 a_tex;
layout(location=2) in vec4 a_color;

out vec2 v_tex;
out vec4 v_color;

void main(void)
{
	gl_Position = u_matrix * vec4(a_position, 1.0);
	v_tex = a_tex;
	v_color = a_color;
}

FRAGMENT:
#version 330
#include Partials/Methods.gl

uniform sampler2D u_texture;
uniform float u_near;
uniform float u_far;

in vec2 v_tex;
in vec4 v_color;

out vec4 o_color;

void main(void)
{
	// apply color value
	o_color = texture(u_texture, v_tex) * v_color;

	// apply depth values
	gl_FragDepth = LinearizeDepth(gl_FragCoord.z, u_near, u_far);
}