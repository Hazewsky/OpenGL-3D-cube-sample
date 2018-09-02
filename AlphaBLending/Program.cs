using System;
using OpenGL;
using Tao.FreeGlut;

namespace AlphaBLending
{
    class Program
    {
        private static int width = 1280, height = 720;
        private static ShaderProgram program;
        //точки
        private static VBO<Vector3> cube;
        private static VBO<Vector2> cubeUV;
        private static VBO<Vector3> cubeNormals;
        //порядок отрисовки точек
        private static VBO<int> cubeElements;
        private static Texture texture;
        private static System.Diagnostics.Stopwatch watch;
        private static float xangle, yangle, zangle;
        private static bool lightning = true, autoRotate = true, fullScreen = false, alpha = true;
        private static bool up = false, down = false, left = false, right = false, forward = false, backward = false;
        private static Vector2 startPos;

        static void Main(string[] args)
        {
            Glut.glutInit();
            //двойная буфферизация и буфер глубины
            //двойная буфферизация позволяет избежать проблем с отрисовкой(юзер видит как оно рисуется)
            //Сначала рисуется на "Заднем" кадре, а потом меняет местами кадры
            //буфер глубины хранит не только инфу о цветах, но и на какой глубине они находятся на экране(избегает overdrawing) Ближайший объект будет отрисован, а на заднем фоне удален
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL tutorial");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);

            Glut.glutMouseFunc(OnMouseMove);
            

            Glut.glutReshapeFunc(OnReshape);
            Glut.glutCloseFunc(OnClose);

            //тестим порядок Z координаты фрагментов. Без нее будет пздц
            //Depth тест не рисует все что на "заднем плане"
             Gl.Disable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            program = new ShaderProgram(VertexShader, FragmentShader);

            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(
                                                0.45f,
                                                (float)width / height,
                //условия когда объект больше не рендерится(ближе 0.1, дальше 1000)
                                                0.1f,
                                                1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(
                                            new Vector3(0, 0, 10), //в 10 юнитах от источника    
                                            Vector3.Zero,//смотри на источниk
                                            Vector3.Up)); //направление вверх?
            program["light_direction"].SetValue(new Vector3(0, 0, 1));
            program["enable_lighting"].SetValue(lightning);
            //загрузка текстуры
            texture = new Texture("glass.bmp");
            //куб
            cube = new VBO<Vector3>(new Vector3[] {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1), //верх
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1), //низ
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1), //фронт
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1), //тыл
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), //лево
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) }); //право
            cubeUV = new VBO<Vector2>(new Vector2[] {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            cubeNormals = new VBO<Vector3>(new Vector3[]{
                new Vector3(0,1,0), new Vector3(0,1,0), new Vector3(0,1,0), new Vector3(0,1,0), //указывает прямо вверх
                new Vector3(0,-1,0), new Vector3(0,-1,0), new Vector3(0,-1,0), new Vector3(0,-1,0), //указывает вниз
                new Vector3(0,0,1), new Vector3(0,0,1), new Vector3(0,0,1), new Vector3(0,0,1), //указывает вперед
                new Vector3(0,0,-1), new Vector3(0,0,-1), new Vector3(0,0,-1), new Vector3(0,0,-1), //указывает назад
                new Vector3(-1,0,0), new Vector3(-1,0,0), new Vector3(-1,0,0), new Vector3(-1,0,0), //указывает влево
                new Vector3(1,0,0), new Vector3(1,0,0), new Vector3(1,0,0), new Vector3(1,0,0) //указывает вправо
            });
            cubeElements = new VBO<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, BufferTarget.ElementArrayBuffer);


            Console.WriteLine("Я консоль для проверки корректности введенных вами данных");
            //начало таймера
            watch = System.Diagnostics.Stopwatch.StartNew();
            Glut.glutMainLoop();
        }


        private static void OnClose()
        {
            cube.Dispose();
            cubeUV.Dispose();
            cubeElements.Dispose();
            cubeNormals.Dispose();
            texture.Dispose();
            program.DisposeChildren = true;
            program.Dispose();
        }
        private static void OnDisplay()
        {

        }
        //главный цикл
        private static void OnRenderFrame()
        {
            //будет считать сколько времени прошло с прошлого кадра
            watch.Stop();
            //Frequency - кол-во тиков в секунду
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            //Console.Write(deltaTime + "\t");
            watch.Restart();

            if (autoRotate)
            {
                xangle += deltaTime;
                yangle += deltaTime;
                zangle += deltaTime;
               
            }
            else
            {
                if (right) yangle += deltaTime;
                if (left) yangle -= deltaTime;
                if (up) xangle += deltaTime;
                if (down) xangle -= deltaTime;
                if (forward) zangle += deltaTime;
                if (backward) zangle -= deltaTime;
                
            }
            //удаляем всю инфу с предыдущего кадра
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // program.Use();
            Gl.UseProgram(program);
            //бинд текстуры
            Gl.BindTexture(texture);
            //рисуем квадрат
            program["model_matrix"].SetValue(
                                            Matrix4.CreateRotationY(yangle) *
                                            Matrix4.CreateRotationX(xangle) *
                                            Matrix4.CreateRotationZ(zangle)
                                            );

            //вкл, выкл освещения в зависимости от 'lightning'
            program["enable_lighting"].SetValue(lightning);
            Gl.BindBufferToShaderAttribute(cube,
                                            program,
                                            "vertexPosition");
            Gl.BindBufferToShaderAttribute(cubeNormals,
                                           program,
                                           "vertexNormal");
            Gl.BindBufferToShaderAttribute(cubeUV,
                                            program,
                                            "vertexUV");
            Gl.BindBuffer(cubeElements);

            Gl.DrawElements(BeginMode.Quads,
                            cubeElements.Count,
                            DrawElementsType.UnsignedInt,
                            IntPtr.Zero);


            //меняем frame и back буффера(двойная буферизация)
            Glut.glutSwapBuffers();

        }

        private static void OnReshape(int width, int height)
        {
            //вгоняем новый размер экрана
            Program.width = width;
            Program.height = height;
            //обновляем projection matrix
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(
                                               0.45f,
                                               (float)width / height,
                //условия когда объект больше не рендерится(ближе 0.1, дальше 1000)
                                               0.1f,
                                               1000f));
        }
        private static void OnKeyboardDown(byte key, int x, int y)
        {
             switch (key)
             {
                //escape
                 case 27:
                     Glut.glutLeaveMainLoop();
                     break;
                 case (byte)'w':
                     up = true;
                     break;
                 case (byte)'a':
                     left = true;
                     break;
                 case (byte)'d':
                     right = true;
                     break;
                 case (byte)'s':
                     down = true;
                     break;
                case (byte)'r':
                    xangle = 0;
                    yangle = 0;
                    zangle = 0;
                    break;
             }
            

        }

        private static void OnMouseMove(int button, int state,
                                int x, int y)
        {
            //buttonDown
            if(state == 0)
            {
                startPos = new Vector2(x, y);

            }
            //buttonUp
            else
            {
                //влево

                if (x < startPos.x)
                {
                    right = false;
                    left = true;
                }
                else if(x > startPos.x)
                {
                    right = true;
                    left = false;
                }
                if (y < startPos.y)
                {
                    up = false;
                    down = true;
                }
                else if (y > startPos.y)
                {
                    up = true;
                    down = false;
                }
               

            }

            if(button == 2)
            {
                right = false;
                left = false;
                up = false;
                down = false;
                forward = false;
                backward = false;
            }
            Glut.glutSwapBuffers();
           
        }
        private static void OnKeyboardUp(byte key, int x, int y)
        {
            if (key == ' ') autoRotate = !autoRotate;
            else if (key == 'l') lightning = !lightning;
            if (key == 'f')
            {
                fullScreen = !fullScreen;
                if (fullScreen) Glut.glutFullScreen();
                else
                {
                    Glut.glutPositionWindow(0, 0);
                    Glut.glutReshapeWindow(1280, 720);
                }
            }
            else if (key == 'w') up = false;
            else if (key == 'a') left = false;
            else if (key == 'd') right = false;
            else if (key == 's') down = false;
            else if (key == 'q') forward = false;
            else if (key == 'e') backward = false;
            else if (key == 'b')
            {
                alpha = !alpha;
                if (alpha)
                {
                    Gl.Enable(EnableCap.Blend);
                    Gl.Disable(EnableCap.DepthTest);
                }
                else
                {
                    Gl.Disable(EnableCap.Blend);
                    Gl.Enable(EnableCap.DepthTest);
                }
            }
        }
        // projection перспектива и подобное
        // view вся инфа о камере
        // model транслирует точки из object space в world space
        //запускается раз в вертекс

        public static string VertexShader = @"
#version 130

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;

out vec3 normal;
out vec2 uv;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    normal = normalize((model_matrix * vec4(floor(vertexNormal), 0)).xyz);
    uv = vertexUV;

    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130

uniform sampler2D texture;
uniform vec3 light_direction;
uniform bool enable_lighting;

in vec3 normal;
in vec2 uv;

out vec4 fragment;

void main(void)
{
    float diffuse = max(dot(normal, light_direction), 0);
    float ambient = 0.3;
    float lighting = (enable_lighting ? max(diffuse, ambient) : 1);

    // add in some blending for tutorial 8 by setting the alpha to 0.5
    fragment = vec4(lighting * texture2D(texture, uv).xyz, 0.5);
}
";
    }
}
