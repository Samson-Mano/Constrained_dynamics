#version 330 core

uniform mat4 modelMatrix;
uniform mat4 panTranslation;
uniform float zoomscale;
uniform float deflscale;
uniform float transparency = 1.0f;
uniform float geom_scale;

layout(location = 0) in vec2 ptmass_quad_position;
layout(location = 1) in vec2 ptmass_center_position;
layout(location = 2) in vec2 textureCoord;
layout(location = 3) in float node_defl;

out vec2 v_textureCoord;
out float v_deflvalue

void main()
{
	// apply zoom scaling and Rotation to model matrix
	mat4 scalingMatrix = mat4(1.0) * zoomscale;
	scalingMatrix[3][3] = 1.0f;
	mat4 scaledModelMatrix = scalingMatrix * modelMatrix;
	
	// Declare variable to store final node center
	vec4 finalquadPosition;
	vec4 finalnodeCenter;
	
	// apply Translation to the ptmass quad vertex position 
	finalquadPosition = scaledModelMatrix * vec4(ptmass_quad_position, 0.0f, 1.0f) * panTranslation;
		
	// apply Translation to the pt mass center
	finalnodeCenter = scaledModelMatrix * vec4(ptmass_center_position, 0.0f, 1.0f) * panTranslation;
	

	v_deflvalue = node_defl;
	v_textureCoord = textureCoord;
	
	// Scale the final position
	vec2 scaled_pt = vec2(finalquadPosition.x - finalnodeCenter.x, finalquadPosition.y - finalnodeCenter.y) / zoomscale;
	
	// Final position passed to fragment shader
	gl_Position = vec4(finalnodeCenter.x + scaled_pt.x, finalnodeCenter.y + scaled_pt.y, 0.0f, 1.0f);
}