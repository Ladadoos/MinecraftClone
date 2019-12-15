#version 400 core
out vec4 FragColor;

in vec2 TexCoord;
in float Light;

uniform sampler2D textureAtlas;

void main()
{
   vec4 albedo = texture(textureAtlas, TexCoord);
   if(albedo.rgb == vec3(1,1,1)){
     discard;
   }
   FragColor = albedo / Light;
}