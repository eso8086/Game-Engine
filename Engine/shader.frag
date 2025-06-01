#version 330 core

in vec2 frag_texCoords;

out vec4 FragColor;

uniform sampler2D uTexture;

void main(){
    FragColor = texture(uTexture, frag_texCoords);
}