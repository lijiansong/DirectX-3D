using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace 高程图并显示周边
{
    public partial class MainForm : Form
    {
        private Device m_device = null;//定义绘图设备
        bool pause = false;

        private Vector3 CamPostion = new Vector3(50.28625f, 1643.985f, 1510.53f);//定义摄像机位置
        private Vector3 CamTarget = new Vector3(648.9199f, -351.2246f, 252.9351f);//定义摄像机目标位置

        private float angleY = 0.01f;//定义绕Y轴旋转变量

        private int mouseLastX, mouseLastY;//记录鼠标按下时的坐标位置
        private bool isRotateByMouse = false;//记录是否由鼠标控制旋转
        private bool isMoveByMouse = false;//记录是否由鼠标控制移动

        private CustomVertex.PositionTextured[] vertices;//定义顶点变量
        private Texture texture;//定义贴图变量
        private Material material;//定义材质变量

        private VertexBuffer vertexBuffer;//定义顶点缓冲变量
        private IndexBuffer indexBuffer;//定义索引缓冲变量
        private int[] indices;//定义索引号变量

        private int xCount = 5, yCount = 4;//定义横向和纵向网格数目
        private float cellHeight = 1f, cellWidth = 1f;//定义单元的宽度和长度

        Material bottomMaterial;//底部材质变量
        Texture bottomTexture;//底部纹理变量
        VertexBuffer bottomVertexBuffer = null;//保存建立底部正方形的顶点

        private VertexBuffer borderFrontVertexBuffer = null;
        private CustomVertex.PositionColored[] borderFrontVertices;//定义前面边缘顶点变量
        private IndexBuffer borderFrontIndexBuffer;//定义前面边缘顶点索引缓冲变量
        private int[] borderFrontIndices;//定义前面边缘顶点的索引号变量

        private VertexBuffer borderBackVertexBuffer = null;
        private CustomVertex.PositionColored[] borderBackVertices;//定义背面边缘顶点变量
        private IndexBuffer borderBackIndexBuffer;//定义背面边缘顶点索引缓冲变量
        private int[] borderBackIndices;//定义背面边缘顶点的索引号变量

        private VertexBuffer borderLeftVertexBuffer = null;
        private CustomVertex.PositionColored[] borderLeftVertices;//定义左面边缘顶点变量
        private IndexBuffer borderLeftIndexBuffer;//定义左面边缘顶点索引缓冲变量
        private int[] borderLeftIndices;//定义左面边缘顶点的索引号变量

        private VertexBuffer borderRightVertexBuffer = null;
        private CustomVertex.PositionColored[] borderRightVertices;//定义右面边缘顶点变量
        private IndexBuffer borderRightIndexBuffer;//定义右面边缘顶点索引缓冲变量
        private int[] borderRightIndices;//定义右面边缘顶点的索引号变量

        public MainForm()
        {
            InitializeComponent();
        }
        public bool InitializeDirect3D()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true; //指定以Windows窗体形式显示
                presentParams.SwapEffect = SwapEffect.Discard; //当前屏幕绘制后它将自动从内存中删除
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                presentParams.EnableAutoDepthStencil = true;
                presentParams.PresentationInterval = PresentInterval.Immediate;
                m_device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams); //实例化device对象
                m_device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnCreateDevice(m_device, null);
                this.OnResetDevice(m_device, null);
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.ToString(), "Error"); //处理异常
                return false;
            }
            return true;
        }
        //导入贴图和材质
        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            material = new Material();
            material.Diffuse = Color.White;
            material.Specular = Color.LightGray;
            material.SpecularSharpness = 15.0F;
            device.Material = material;
            texture = TextureLoader.FromFile(device, @"F:\\workdir\\VC# Based DirectX\\RoadTexture.jpg");

            //底部材料和贴图
            bottomMaterial = new Material();
            bottomMaterial.Ambient = Color.FromArgb(200, 255, 255, 255);
            bottomMaterial.Diffuse = Color.FromArgb(200, 255, 255, 255);
            bottomTexture = TextureLoader.FromFile(device, "F:\\workdir\\VC# Based DirectX\\BottomTexture.bmp");
            bottomVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), 6, device, 0, CustomVertex.PositionNormalTextured.Format, Pool.Default);
            bottomVertexBuffer.Created += new System.EventHandler(this.OnCreateBottomVertexBuffer);
            this.OnCreateBottomVertexBuffer(device, null);
        }
        //底部顶点
        public void OnCreateBottomVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.PositionNormalTextured[] bottomVerts = (CustomVertex.PositionNormalTextured[])bottomVertexBuffer.Lock(0, 0);//绘制底面正方形的6个顶点
            string bitmapPath = @"F:\\workdir\\VC# Based DirectX\\RoadHeight.BMP";
            float minHeight = GetMinHeight(bitmapPath);
            float xWidth = GetBitMapWidth(bitmapPath);
            float yHeight = GetBitMapHeight(bitmapPath);
            bottomVerts[0].Position = new Vector3(-100.0f, minHeight - 5.0f, -100.0f);
            bottomVerts[0].Normal = new Vector3(0, 0, -1);
            bottomVerts[0].Tu = 0.0f;//顶点0纹理坐标Tu
            bottomVerts[0].Tv = 1.0f;//纹理图片沿Y轴方向重复贴图50次

            bottomVerts[1].Position = new Vector3(-100.0f, minHeight - 5.0f, yHeight + 100.0f);
            bottomVerts[1].Normal = new Vector3(0, 0, -1);
            bottomVerts[1].Tu = 0.0f;
            bottomVerts[1].Tv = 0.0f;

            bottomVerts[2].Position = new Vector3(xWidth + 100.0f, minHeight - 5.0f, yHeight + 100.0f);
            bottomVerts[2].Normal = new Vector3(0, 0, -1);
            bottomVerts[2].Tu = 1.0f;
            bottomVerts[2].Tv = 0.0f;

            bottomVerts[3].Position = new Vector3(-100.0f, minHeight - 5.0f, -100.0f);
            bottomVerts[3].Normal = new Vector3(0, 0, -1);
            bottomVerts[3].Tu = 0.0f;
            bottomVerts[3].Tv = 1.0f;

            bottomVerts[4].Position = new Vector3(xWidth + 100.0f, minHeight - 5.0f, yHeight + 100.0f);
            bottomVerts[4].Normal = new Vector3(0, 0, -1);
            bottomVerts[4].Tu = 1.0f;
            bottomVerts[4].Tv = 0.0f;

            bottomVerts[5].Position = new Vector3(xWidth + 100.0f, minHeight - 5.0f, -100.0f);
            bottomVerts[5].Normal = new Vector3(0, 0, -1);
            bottomVerts[5].Tu = 1.0f;
            bottomVerts[5].Tv = 1.0f;

            bottomVertexBuffer.Unlock();
        }
        //避免精度损失
        public float GetBitMapHeight(string bitmapPath)
        {
            Bitmap bitmap = new Bitmap(bitmapPath);
            xCount = (bitmap.Width - 1) / 2;
            yCount = xCount;
            cellWidth = bitmap.Width / xCount;
            cellHeight = bitmap.Height / yCount;
            return (float)(yCount * cellHeight);
        }
        //避免精度损失
        public float GetBitMapWidth(string bitmapPath)
        {
            Bitmap bitmap = new Bitmap(bitmapPath);
            xCount = (bitmap.Width - 1) / 2;
            yCount = xCount;
            cellWidth = bitmap.Width / xCount;
            cellHeight = bitmap.Height / yCount;
            return (float)(xCount * cellWidth);
        }
        //获得高度图Y方向上的最小高度值 
        public float GetMinHeight(string bitmapPath)
        {
            float minHeight = 6553500.0f;
            Bitmap bitmap = new Bitmap(bitmapPath);
            xCount = (bitmap.Width - 1) / 2;
            yCount = xCount;
            cellWidth = bitmap.Width / xCount;
            cellHeight = bitmap.Height / yCount;
            for (int i = 0; i < yCount + 1; i++)
                for (int j = 0; j < xCount + 1; j++)
                {
                    Color color = bitmap.GetPixel((int)(j * cellWidth), (int)(i * cellHeight));
                    float height = float.Parse(color.R.ToString()) +
                    float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                    height /= 10;
                    if (height < minHeight)
                        minHeight = height;
                }
            return minHeight;
        }
        /**
         * 通过Bitmap类获得BMP图片对象，然后根据其Width属性和Height属性设置生成三角网中横向和纵向单元数，
         * 同时设置单元尺寸，在设置高程值中通过获取图片中相应位置的颜色值，
         * 然后通过一个相对比例得到一个相对的高程值，最后将该值赋予定义的顶点坐标值
         */
        public void OnResetDevice(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            string bitmapPath = @"F:\\workdir\\VC# Based DirectX\\RoadHeight.BMP";
            Bitmap bitmap = new Bitmap(bitmapPath);
            xCount = (bitmap.Width - 1) / 2;
            yCount = xCount;
            cellWidth = bitmap.Width / xCount;
            cellHeight = bitmap.Height / yCount;

            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured), (xCount + 1) * (yCount + 1), device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            vertices = new CustomVertex.PositionTextured[(xCount + 1) * (yCount + 1)];//定义顶点
            for (int i = 0; i < yCount + 1; i++)
            {
                for (int j = 0; j < xCount + 1; j++)
                {
                    Color color = bitmap.GetPixel((int)(j * cellWidth), (int)(i * cellHeight));
                    float height = float.Parse(color.R.ToString()) + float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                    height /= 10;
                    vertices[j + i * (xCount + 1)].Position = new Vector3(j * cellWidth, height, i * cellHeight);
                    vertices[j + i * (xCount + 1)].Tu = (float)j / (xCount + 1);
                    vertices[j + i * (xCount + 1)].Tv = (float)i / (yCount + 1);
                }
            }
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
            CamTarget = new Vector3(bitmap.Width / 2, 0f, bitmap.Height / 2);//设置摄像机目标位置

            indexBuffer = new IndexBuffer(typeof(int), 6 * xCount * yCount, device, Usage.WriteOnly, Pool.Default);
            indices = new int[6 * xCount * yCount];
            for (int i = 0; i < yCount; i++)
            {
                for (int j = 0; j < xCount; j++)
                {
                    indices[6 * (j + i * xCount)] = j + i * (xCount + 1);
                    indices[6 * (j + i * xCount) + 1] = j + (i + 1) * (xCount + 1);
                    indices[6 * (j + i * xCount) + 2] = j + i * (xCount + 1) + 1;
                    indices[6 * (j + i * xCount) + 3] = j + i * (xCount + 1) + 1;
                    indices[6 * (j + i * xCount) + 4] = j + (i + 1) * (xCount + 1);
                    indices[6 * (j + i * xCount) + 5] = j + (i + 1) * (xCount + 1) + 1;
                }
            }
            indexBuffer.SetData(indices, 0, LockFlags.None);
            /*******第1个四周边界********/
            float minY = GetMinHeight(bitmapPath) - 5.0f;
            borderFrontVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
                2 * (xCount + 1),//四周的一个面封闭总共所需要的顶点的数目
                device,
                Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionColored.Format,
                Pool.Default);
            borderFrontVertices = new CustomVertex.PositionColored[2 * (xCount + 1)];//定义顶点
            int k;
            for (k = 0; k < xCount + 1; k++)//X轴上的点的定义
            {
                borderFrontVertices[k].Position = new Vector3(k * cellWidth, minY, 0.0f);
                borderFrontVertices[k].Color = System.Drawing.Color.Red.ToArgb();
            }
            for (; k < 2 * (xCount + 1); k++)//高程图上的边界点的定义
            {
                Color color = bitmap.GetPixel((int)((k - xCount - 1) * cellWidth), 0);
                float height = float.Parse(color.R.ToString()) + float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                height /= 10;
                borderFrontVertices[k].Position = new Vector3((k - xCount - 1) * cellWidth, height, 0);//i * cellHeight=0
                borderFrontVertices[k].Color = System.Drawing.Color.Aqua.ToArgb();
            }

            borderFrontVertexBuffer.SetData(borderFrontVertices, 0, LockFlags.None);

            borderFrontIndexBuffer = new IndexBuffer(typeof(int),
                6 * xCount * 1,
                device,
                Usage.WriteOnly,
                Pool.Default);
            borderFrontIndices = new int[6 * xCount * 1];//初始化索引顶点

            for (int j = 0; j < xCount; j++)
            {
                borderFrontIndices[6 * (j)] = j;
                borderFrontIndices[6 * (j) + 1] = j + (xCount + 1);
                borderFrontIndices[6 * (j) + 2] = j + 1;
                borderFrontIndices[6 * (j) + 3] = j + 1;
                borderFrontIndices[6 * (j) + 4] = j + (xCount + 1);
                borderFrontIndices[6 * (j) + 5] = j + (xCount + 1) + 1;
            }
            borderFrontIndexBuffer.SetData(borderFrontIndices, 0, LockFlags.None);

            /**第2个四周边界**/
            borderBackVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
                2 * (xCount + 1),//四周的一个面封闭总共所需要的顶点的数目
                device,
                Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionColored.Format,
                Pool.Default);
            borderBackVertices = new CustomVertex.PositionColored[2 * (xCount + 1)];//定义顶点
            for (k = 0; k < xCount + 1; k++)//X轴平行方向上的点的定义
            {
                borderBackVertices[k].Position = new Vector3(k * cellWidth, minY, yCount * cellHeight);
                borderBackVertices[k].Color = System.Drawing.Color.Aqua.ToArgb();
            }
            for (; k < 2 * (xCount + 1); k++)//高程图上的边界点的定义
            {
                Color color = bitmap.GetPixel((int)((k - xCount - 1) * cellWidth), (int)(yCount*cellHeight));//不能直接写死
                float height = float.Parse(color.R.ToString()) + float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                height /= 10;
                borderBackVertices[k].Position = new Vector3((k - xCount - 1) * cellWidth, height, yCount * cellHeight);
                borderBackVertices[k].Color = System.Drawing.Color.Aqua.ToArgb();
            }

            borderBackVertexBuffer.SetData(borderBackVertices, 0, LockFlags.None);

            borderBackIndexBuffer = new IndexBuffer(typeof(int),
                6 * xCount * 1,
                device,
                Usage.WriteOnly,
                Pool.Default);
            borderBackIndices = new int[6 * xCount * 1];//初始化索引顶点

            for (int j = 0; j < xCount; j++)
            {
                borderBackIndices[6 * (j)] = j;
                borderBackIndices[6 * (j) + 1] = j + (xCount + 1);
                borderBackIndices[6 * (j) + 2] = j + 1;
                borderBackIndices[6 * (j) + 3] = j + 1;
                borderBackIndices[6 * (j) + 4] = j + (xCount + 1);
                borderBackIndices[6 * (j) + 5] = j + (xCount + 1) + 1;
            }
            borderBackIndexBuffer.SetData(borderBackIndices, 0, LockFlags.None);

            int diff = yCount-xCount;
            /**第3个四周边界**/
            borderLeftVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
                2 * (yCount + 1),//四周的一个面封闭总共所需要的顶点的数目
                device,
                Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionColored.Format,
                Pool.Default);
            borderLeftVertices = new CustomVertex.PositionColored[2 * (yCount + 1)];//定义顶点
            
            for (k = 0; k < yCount + 1; k++)//Z轴平行方向上的点的定义
            {
                borderLeftVertices[k].Position = new Vector3(0, minY, k * cellHeight);
                borderLeftVertices[k].Color = System.Drawing.Color.Aqua.ToArgb();
            }
            for (; k < 2 * (yCount + 1); k++)//高程图上的边界点的定义
            {
                Color color = bitmap.GetPixel(0, (int)((k - yCount - 1) * cellHeight));//边界点不能直接传进去写死，容易产生溢出的异常
                float height = float.Parse(color.R.ToString()) + float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                height /= 10;
                borderLeftVertices[k].Position = new Vector3(0, height, (k - yCount - 1) * cellHeight);
                borderLeftVertices[k].Color = System.Drawing.Color.Aqua.ToArgb();
            }

            borderLeftVertexBuffer.SetData(borderLeftVertices, 0, LockFlags.None);

            borderLeftIndexBuffer = new IndexBuffer(typeof(int),
                6 * yCount * 1,
                device,
                Usage.WriteOnly,
                Pool.Default);
            borderLeftIndices = new int[6 * yCount * 1];//初始化索引顶点

            for (int j = 0; j < yCount; j++)
            {
                borderLeftIndices[6 * (j)] = j;
                borderLeftIndices[6 * (j) + 1] = j + (yCount + 1);
                borderLeftIndices[6 * (j) + 2] = j + 1;
                borderLeftIndices[6 * (j) + 3] = j + 1;
                borderLeftIndices[6 * (j) + 4] = j + (yCount + 1);
                borderLeftIndices[6 * (j) + 5] = j + (yCount + 1) + 1;
            }
            borderLeftIndexBuffer.SetData(borderLeftIndices, 0, LockFlags.None);
            //MessageBox.Show(borderLeftIndices[0]+","+borderLeftIndices[1]+","+borderLeftIndices[2]);

            /**第4个四周边界**/
            borderRightVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
                2 * (yCount + 1),//四周的一个面封闭总共所需要的顶点的数目
                device,
                Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionColored.Format,
                Pool.Default);
            borderRightVertices = new CustomVertex.PositionColored[2 * (yCount + 1)];//定义顶点
            for (k = 0; k < yCount + 1; k++)//Z轴平行方向上的点的定义
            {
                borderRightVertices[k].Position = new Vector3(xCount * cellWidth, minY, k * cellHeight);
                borderRightVertices[k].Color = System.Drawing.Color.Aqua.ToArgb();
            }
            for (; k < 2 * (yCount + 1); k++)//高程图上的边界点的定义
            {
                Color color = bitmap.GetPixel((int)(xCount * cellWidth), (int)((k - yCount - 1) * cellHeight));//边界点不能直接传进去写死，容易产生溢出和精度损失
                float height = float.Parse(color.R.ToString()) + float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                height /= 10;
                borderRightVertices[k].Position = new Vector3(xCount * cellWidth, height, (k - yCount - 1) * cellHeight);
                borderRightVertices[k].Color = System.Drawing.Color.Aqua.ToArgb();
            }

            borderRightVertexBuffer.SetData(borderRightVertices, 0, LockFlags.None);

            borderRightIndexBuffer = new IndexBuffer(typeof(int),
                6 * yCount * 1,
                device,
                Usage.WriteOnly,
                Pool.Default);
            borderRightIndices = new int[6 * yCount * 1];//初始化索引顶点

            for (int j = 0; j < yCount; j++)
            {
                borderRightIndices[6 * (j)] = j;
                borderRightIndices[6 * (j) + 1] = j + (yCount + 1);
                borderRightIndices[6 * (j) + 2] = j + 1;
                borderRightIndices[6 * (j) + 3] = j + 1;
                borderRightIndices[6 * (j) + 4] = j + (yCount + 1);
                borderRightIndices[6 * (j) + 5] = j + (yCount + 1) + 1;
            }
            borderRightIndexBuffer.SetData(borderRightIndices, 0, LockFlags.None);

            device.SamplerState[0].MagFilter = TextureFilter.Linear;//使用纹理滤波器进行线性滤波

        }
        
        public void Render()//渲染
        {
            if (m_device == null)   //如果device为空则不渲染
                return;
            if (pause) //如果窗口被切换或者最小化则停止渲染，减少CPU压力
                return;
            m_device.Transform.World = Matrix.Identity;
            
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            m_device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1.0f, 6500.0f);
            m_device.Transform.View = viewMatrix;

            m_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.DarkSlateBlue, 1.0f, 0);  //清除windows界面为深蓝色
            m_device.BeginScene();
            //在此添加渲染图形代码
            m_device.RenderState.CullMode = Cull.None;
            m_device.RenderState.FillMode = FillMode.Solid;
            m_device.RenderState.Lighting = false;
            m_device.SetTexture(0, texture);//设置贴图

            m_device.VertexFormat = CustomVertex.PositionTextured.Format;
            m_device.SetStreamSource(0, vertexBuffer, 0);
            m_device.Indices = indexBuffer;
            m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (xCount + 1) * (yCount + 1), 0, indices.Length / 3);

            m_device.Transform.World = Matrix.Translation(0.0f, 0.0f, 0.0f);//底部世界变换
            m_device.Material = bottomMaterial;//底部使用的材质

            StateBlock sb = new StateBlock(m_device, StateBlockType.All);
            sb.Capture();//将device以前的设置保存到sb

            m_device.RenderState.DiffuseMaterialSource = ColorSource.Material;
            m_device.RenderState.AlphaBlendEnable = true;
            m_device.SetTexture(0, bottomTexture);
            m_device.TextureState[0].ColorOperation = TextureOperation.Modulate;
            m_device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
            m_device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
            m_device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            m_device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            m_device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;
            m_device.RenderState.SourceBlend = Blend.SourceColor;
            m_device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            m_device.SetStreamSource(0, bottomVertexBuffer, 0);
            m_device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            sb.Apply();
            /*第1个边界三角形*/
            m_device.SetStreamSource(0, borderFrontVertexBuffer, 0);
            m_device.VertexFormat = CustomVertex.PositionColored.Format;
            m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2 * (xCount + 1), 0, borderFrontIndices.Length / 3);
            /*第2个边界三角形*/
            m_device.SetStreamSource(0, borderBackVertexBuffer, 0);
            m_device.VertexFormat = CustomVertex.PositionColored.Format;
            m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2 * (xCount + 1), 0, borderBackIndices.Length / 3);
            /*第3个边界三角形*/
            m_device.SetStreamSource(0, borderLeftVertexBuffer, 0);
            m_device.VertexFormat = CustomVertex.PositionColored.Format;
            m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2 * (yCount + 1), 0, borderLeftIndices.Length / 3);
            /*第4个边界三角形*/
            m_device.SetStreamSource(0, borderRightVertexBuffer, 0);
            m_device.VertexFormat = CustomVertex.PositionColored.Format;
            m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2 * (yCount + 1), 0, borderRightIndices.Length / 3);

            m_device.EndScene();
            m_device.Present();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeDirect3D();
            this.Show();
            Render();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Vector4 tempV4;
            Matrix currentView = m_device.Transform.View;//当前摄像机的视图矩阵
            switch (e.KeyCode)
            {
                case Keys.Left:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                            Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), -angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Right:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                            Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Up:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(m_device.Transform.View.M11
                       , m_device.Transform.View.M21, m_device.Transform.View.M31), -angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Down:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(m_device.Transform.View.M11
                       , m_device.Transform.View.M21, m_device.Transform.View.M31), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Add:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(0.95f);
                    CamPostion.Add(CamTarget);
                    break;
                case Keys.Subtract:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(1.05f);
                    CamPostion.Add(CamTarget);
                    break;
            }
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            m_device.Transform.View = viewMatrix;
            Render();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isRotateByMouse = true;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isMoveByMouse = true;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            isRotateByMouse = false;
            isMoveByMouse = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isRotateByMouse)
            {
                Matrix currentView = m_device.Transform.View;//当前摄像机的视图矩阵
                float tempAngleY = 2 * (float)(e.X - mouseLastX) / this.Width;
                CamPostion.Subtract(CamTarget);

                Vector4 tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), tempAngleY)));
                CamPostion.X = tempV4.X;
                CamPostion.Y = tempV4.Y;
                CamPostion.Z = tempV4.Z;

                float tempAngleX = 4 * (float)(e.Y - mouseLastY) / this.Height;
                tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M11, currentView.M21, currentView.M31), tempAngleX)));
                CamPostion.X = tempV4.X + CamTarget.X;
                CamPostion.Y = tempV4.Y + CamTarget.Y;
                CamPostion.Z = tempV4.Z + CamTarget.Z;
                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                m_device.Transform.View = viewMatrix;

                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
            else if (isMoveByMouse)
            {
                Matrix currentView = m_device.Transform.View;//当前摄像机的视图矩阵
                float moveFactor = 0.01f;
                CamTarget.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamTarget.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamTarget.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                CamPostion.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamPostion.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamPostion.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                m_device.Transform.View = viewMatrix;
                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
        }
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            float scaleFactor = -(float)e.Delta / 2000 + 1f;
            CamPostion.Subtract(CamTarget);
            CamPostion.Scale(scaleFactor);
            CamPostion.Add(CamTarget);
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            m_device.Transform.View = viewMatrix;
            Render();
        }

        private void Form1_Resize(object sender, EventArgs e)//窗口切换时停止渲染
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }
    }
}
