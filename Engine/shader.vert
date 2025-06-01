#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoords;

uniform mat4 uTrans;

out vec2 frag_texCoords;

void main(){
    gl_Position = uTrans * vec4(aPosition, 1.0);
    frag_texCoords = aTexCoords;
}
