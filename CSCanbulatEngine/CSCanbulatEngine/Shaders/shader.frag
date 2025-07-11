#version 330 core

out vec4 FragColor;

//Reciever texture coords
in vec2 vTexCoord;

//uniform for object color
uniform vec4 uColor;

//uniform for texture sampler
uniform sampler2D uTexture;

void main()
{
    //Sample color at texture coordinate
    vec4 textureColor = texture(uTexture, vTexCoord);
    
    //Multiply texture colour by uniform color
    //Allows for tinting colors or just show a solid color if its white
    FragColor = textureColor * uColor; // <- Should be orange colour
}