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

namespace VertexBufferTest
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
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
            Device dev = (Device)sender;
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColored),
            3, dev, 0, CustomVertex.TransformedColored.Format, Pool.Default);
            //事件的预订，指定OnCreateVertexBuffer函数是vertexBuffer.Created事件函数
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null); //创建顶点数组

        }

        public void OnResetDevice(object sender, EventArgs e)
        { }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 	//如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Blue, 1.0f, 0);
            device.BeginScene();	//开始渲染
            device.SetStreamSource(0, vertexBuffer, 0); //使用vertexBuffer中定义的顶点
            device.VertexFormat = CustomVertex.TransformedColored.Format;	//顶点格式
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            device.EndScene();		//渲染结束
            device.Present();		//更新显示区域，把后备缓存的D图形送到图形卡的显存中显示

        }

        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.TransformedColored[] verts =
                                (CustomVertex.TransformedColored[])vertexBuffer.Lock(0, 0);
            verts[0].X = 150;
            verts[0].Y = 50; 					//顶点0位置
            verts[0].Z = 0.5f;
            verts[0].Rhw = 1;
            verts[0].Color = System.Drawing.Color.Aqua.ToArgb();		//顶点0颜色
            verts[1].X = 250;
            verts[1].Y = 250;
            verts[1].Z = 0.5f;
            verts[1].Rhw = 1;
            verts[1].Color = System.Drawing.Color.Brown.ToArgb();
            verts[2].X = 50;
            verts[2].Y = 250;
            verts[2].Z = 0.5f;
            verts[2].Rhw = 1;
            verts[2].Color = System.Drawing.Color.LightPink.ToArgb();
            vertexBuffer.Unlock();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            Show();
            Render();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }
    }
}
