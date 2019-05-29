﻿namespace AGS.API
{
    /// <summary>
    /// Shader mode (currently only fragment and vertex shaders are supported).
    /// </summary>
    public enum ShaderMode
    {
        FragmentShader,
        VertexShader,
        GeometryShader,
        GeometryShaderExt,
        TessEvaluationShader,
        TessControlShader,
        ComputeShader,
    }

    /// <summary>
    /// A buffer containing shader variables.
    /// </summary>
    public struct ShaderVarsBuffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:AGS.API.ShaderVar"/> struct.
        /// </summary>
        /// <param name="stage">The shader mode that the variable will be passed to.</param>
        /// <param name="name">Name of the variable.</param>
        /// <param name="numVars">Number of variables in the buffer (all variables are of type float).</param>
        public ShaderVarsBuffer(ShaderMode stage, string name, int numVars = 1)
        {
            Stage = stage;
            Vars = new float[numVars];
            Name = name;
        }

        /// <summary>
        /// Gets the shader mode that the variable will be passed to.
        /// </summary>
        /// <value>The stage.</value>
        public ShaderMode Stage { get; private set; }
        /// <summary>
        /// Gets the type of the variable.
        /// </summary>
        /// <value>The type of the variable.</value>
        public float[] Vars { get; private set; }
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }
    }

    /// <summary>
    /// Represents a shader which is a user defined program that can be written to directly affect what's rendered on the screen.
    /// See here: https://www.opengl.org/wiki/Shader
    /// We're currently supporting vertex &amp; fragment shaders.
    /// </summary>
    public interface IShader
	{
        /// <summary>
        /// Gets the program identifier for the shader (used by GLSL).
        /// </summary>
        /// <value>The program identifier.</value>
        int ProgramId { get; }
        /// <summary>
        /// Compiles this shader. 
        /// </summary>
        /// <param name="shaderVars">The variables that the shader accepts (in the order they are written in the shader).</param>
        /// <returns>Will return itself if compiled successfully or null if there are compilation errors (those will be logged to the screen).</returns>
        /// <example>
        /// A common pattern will look like this:
        /// <code language="lang-csharp">
        /// const string VERTEX_SHADER = "GLSL vertex shader code goes here";
        /// const string FRAGMENT_SHADER = "GLSL fragment shader code goes here";
        /// IShader shader = GLShader.FromText(VERTEX_SHADER, FRAGMENT_SHADER).Compile();
        /// if (shader == null)
        /// {
        ///   //oh oh, it did not compile!
        /// }
        /// </code>
        /// There's also a possibility of loading the shader from a resource/file:
        /// <code language="lang-csharp">
        /// IShader shader = await GLShader.FromResource("vertexShader.glsl", "fragmentShader.glsl");
        /// </code>
        /// </example>
        IShader Compile(params ShaderVarsBuffer[] shaderVars);
        /// <summary>
        /// Binds the shader. This must be performed from the rendering thread.
        /// Once the shader is bound, all rendering activities will run through the shader until it is unbound (or when another shader is bound).
        /// Setting variables for the shader must also be performed when the shader is bound.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// //assuming shader was already successfully compiled...
        /// shader.Bind(); //shader is now in control!        
        /// </code>
        /// </example>
        void Bind();
        /// <summary>
        /// Unbinds this shader, leaving control to the normal renderer. This must be performed from the rendering thread.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// shader.Bind(); //shader is now in control!
        /// await Task.Delay(2000);
        /// shader.Unbind(); //shader is done, back to normal rendering.
        /// </code>
        /// </example>
        void Unbind();
        /// <summary>
        /// Sets a float variable in the shader program.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="x">The value.</param>
        /// <returns>True if the variable was found and set</returns>
        /// <example>
        /// <code language="lang-csharp">
        /// //In GLSL shader:
        /// uniform float myVariable;
        /// //Do stuff with myVariable...
        /// ...
        /// ...
        /// //In AGS code:
        /// shader.Bind();
        /// shader.SetVariable("myVariable", 5.5f);
        /// </code>
        /// </example>
        bool SetVariable(string name, float x);
        /// <summary>
        /// Sets a 2d vector variable in the shader program.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <returns>True if the variable was found and set</returns>
        /// <example>
        /// <code language="lang-csharp">
        /// //In GLSL shader:
        /// uniform vec2 myVariable;
        /// //Do stuff with myVariable...
        /// ...
        /// ...
        /// //In AGS code:
        /// shader.Bind();
        /// shader.SetVariable("myVariable", 5.5f, 3f);
        /// </code>
        /// </example>
		bool SetVariable(string name, float x, float y);
        /// <summary>
        /// Sets a 3d vector variable in the shader program.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        /// <returns>True if the variable was found and set</returns>
        /// <example>
        /// <code language="lang-csharp">
        /// //In GLSL shader:
        /// uniform vec3 myVariable;
        /// //Do stuff with myVariable...
        /// ...
        /// ...
        /// //In AGS code:
        /// shader.Bind();
        /// shader.SetVariable("myVariable", 5.5f, 3f, 0.1f);
        /// </code>
        /// </example>
		bool SetVariable(string name, float x, float y, float z);
        /// <summary>
        /// Sets a 4d vector variable in the shader program.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        /// <param name="w">The w value.</param>
        /// <returns>True if the variable was found and set</returns>
        /// <example>
        /// <code language="lang-csharp">
        /// //In GLSL shader:
        /// uniform vec4 myVariable;
        /// //Do stuff with myVariable...
        /// ...
        /// ...
        /// //In AGS code:
        /// shader.Bind();
        /// shader.SetVariable("myVariable", 5.5f, 3f, 0.1f, 2.5f);
        /// </code>
        /// </example>
		bool SetVariable(string name, float x, float y, float z, float w);
        /// <summary>
        /// Sets a color variable in the shader program (GLSL sees it as a 4d vector).
        /// This is just a convenience method that can be used instead of the 4d version for colors.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="c">The color.</param>        
        /// <returns>True if the variable was found and set</returns>
        /// <example>
        /// <code language="lang-csharp">
        /// //In GLSL shader:
        /// uniform vec4 myColor;
        /// //Do stuff with myColor...
        /// ...
        /// ...
        /// //In AGS code:
        /// shader.Bind();
        /// shader.SetVariable("myColor", Colors.White);
        /// </code>
        /// </example>
		bool SetVariable(string name, Color c);
        /// <summary>
        /// Sets a texture variable in the shader program.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="texture">The texture.</param>
        /// <returns>True if the variable was found and set</returns>
        bool SetTextureVariable(string name, int texture);
	}
}

