#version 330 core

//Vertex position and Texture coordinates
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;

// out to pass texture coords to fragment shader
out vec2 vTexCoord;

//Model matrix
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    
    vTexCoord = aTexCoord;
}