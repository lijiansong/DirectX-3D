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

namespace 空心圆柱添加背景
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
        Material mtrl;
        Texture texture = null;
        VertexBuffer vertexBuffer1 = null;
        Texture texture1 = null;

        public Form1()
        {
            InitializeComponent();
        }

        public bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;				//不是全屏显示，在一个窗口显示
                presentParams.SwapEffect = SwapEffect.Discard;		 //后备缓存交换的方式
                presentParams.EnableAutoDepthStencil = true;			 //允许使用自动深度模板测试
                //深度缓冲区单元为16位二进制数
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                device = new Device(0, DeviceType.Hardware, this, 	 //建立设备类对象
          CreateFlags.SoftwareVertexProcessing, presentParams);
                //设置设备重置事件(device.DeviceReset)事件函数为this.OnResetDevice
                device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnCreateDevice(device, null);//自定义方法，初始化Device的工作放到这个方法中
                this.OnResetDevice(device, null);//调用设备重置事件(device.DeviceReset)事件函数
            }		//设备重置事件函数要设置Device参数，初始函数中必须调用该函数
            catch (DirectXException)
            {
                return false;
            }
            return true;
        }

        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;			//阴影部分是所作的修改
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), 100, dev,
            Usage.WriteOnly, CustomVertex.PositionNormalTextured.Format, Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);
            mtrl = new Material();
            mtrl.Diffuse = System.Drawing.Color.Yellow;		//物体的颜色
            mtrl.Ambient = System.Drawing.Color.Red;			//反射环境光的颜色
            texture = TextureLoader.FromFile(dev, Application.StartupPath + @"\..\..\..\p1.JPG");
            vertexBuffer1 = new VertexBuffer(typeof(CustomVertex.TransformedTextured), 4, dev, 0, CustomVertex.TransformedTextured.Format, Pool.Default);
            vertexBuffer1.Created += new EventHandler(vertexBuffer1_Created);
            this.vertexBuffer1_Created(vertexBuffer1, null);
            texture1 = TextureLoader.FromFile(dev, Application.StartupPath + @"\..\..\..\p2.jpg");
        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.None;				//取消背面剔除
            device.RenderState.ZBufferEnable = true;			//打开Z缓冲
            device.RenderState.Lighting = true;				//打开灯光
            SetupLights();

        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 					//如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.WhiteSmoke, 1.0f, 0);
            SetupMatrices();
            device.BeginScene();					//开始渲染
            device.SetTexture(0, texture1);			//这段代码必须放在渲染圆柱体代码之前
            device.RenderState.ZBufferEnable = false;
            device.SetStreamSource(0, vertexBuffer1, 0);
            device.VertexFormat = CustomVertex.TransformedTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
            device.RenderState.ZBufferEnable = true;
            device.SetTexture(0, texture);
            device.TextureState[0].ColorOperation = TextureOperation.Modulate;
            device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
            device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
            device.TextureState[0].AlphaOperation = TextureOperation.Disable;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, (4 * 25) - 2);
            device.EndScene();		//渲染结束
            device.Present();		//更新显示区域，把后备缓存的D图形送到图形卡的显存中显示


        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.PositionNormalTextured[] verts =
                        (CustomVertex.PositionNormalTextured[])vertexBuffer.Lock(0, 0);
            for (int i = 0; i < 50; i++)
            {
                float theta = (float)(2 * Math.PI * i) / 49;
                verts[2 * i].Position = new Vector3((float)Math.Sin(theta), -1, (float)Math.Cos(theta));
                verts[2 * i].Normal = new Vector3((float)Math.Sin(theta), 0, (float)Math.Cos(theta));
                verts[2 * i].Tu = ((float)i) / (50 - 1);
                verts[2 * i].Tv = 1.0f;
                verts[2 * i + 1].Position = new Vector3((float)Math.Sin(theta), 1, (float)Math.Cos(theta));
                verts[2 * i + 1].Normal = new Vector3((float)Math.Sin(theta), 0, (float)Math.Cos(theta));
                verts[2 * i + 1].Tu = ((float)i) / (50 - 1);
                verts[2 * i + 1].Tv = 0.0f;
            }
            vertexBuffer.Unlock();
        }

        private void SetupMatrices()
        {
            device.Transform.World = Matrix.RotationAxis(new Vector3((float)Math.Cos(Environment.TickCount / 250.0f), 1, (float)Math.Sin(Environment.TickCount / 250.0f)), Environment.TickCount / 3000.0f);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, -5.0f),
                        new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 1.0f, 100.0f);
        }

        private void SetupLights()
        {
            device.Material = mtrl;
            device.Lights[0].Type = LightType.Directional;
            device.Lights[0].Diffuse = System.Drawing.Color.White;
            device.Lights[0].Direction = new Vector3((float)Math.Cos(Environment.TickCount / 250.0f),
                                    1.0f, (float)Math.Sin(Environment.TickCount / 250.0f));
            device.Lights[0].Enabled = true;
            device.RenderState.Ambient = System.Drawing.Color.FromArgb(0x404040);
        }

        void vertexBuffer1_Created(object sender, EventArgs e)
        {
            CustomVertex.TransformedTextured[] verts =
        (CustomVertex.TransformedTextured[])vertexBuffer1.Lock(0, 0);
            verts[0].Position = new Vector4(0, 0, 0, 1);
            verts[0].Tu = 0.0f;
            verts[0].Tv = 0.0f;
            verts[1].Position = new Vector4(this.Width, 0, 0, 1);
            verts[1].Tu = 1.0f;
            verts[1].Tv = 0.0f;
            verts[2].Position = new Vector4(this.Width, this.Height, 0, 1);
            verts[2].Tu = 1.0f;
            verts[2].Tv = 1.0f;
            verts[3].Position = new Vector4(0, this.Height, 0, 1);
            verts[3].Tu = 0.0f;
            verts[3].Tv = 1.0f;
            vertexBuffer1.Unlock();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }

    }
}
