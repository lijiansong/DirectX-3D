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

namespace Triangle01
{
    public partial class Form1 : Form
    {
        private Device device = null;
        CustomVertex.TransformedColored[] verts = null, verts1 = null;		//点数组
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
            verts = new CustomVertex.TransformedColored[6];
            verts[0].Position = new Vector4(10.0f, 10.0f, 0.5f, 1.0f);//第0点
            verts[0].Color = Color.Aqua.ToArgb();
            verts[1].Position = new Vector4(210.0f, 10.0f, 0.5f, 1.0f); //第1点
            verts[1].Color = Color.Brown.ToArgb();
            verts[2].Position = new Vector4(110.0f, 60.0f, 0.5f, 1.0f); //第2点
            verts[2].Color = Color.LightPink.ToArgb();
            verts[3].Position = new Vector4(210.0f, 210.0f, 0.5f, 1.0f); //第3点
            verts[3].Color = Color.Aqua.ToArgb();
            verts[4].Position = new Vector4(110.0f, 160.0f, 0.5f, 1.0f); //第4点
            verts[4].Color = Color.Brown.ToArgb();
            verts[5].Position = new Vector4(10.0f, 210.0f, 0.5f, 1.0f); //第5点
            verts[5].Color = Color.LightPink.ToArgb();

        }

        public void OnResetDevice(object sender, EventArgs e)
        {  }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 	//如果未建立设备对象，退出
                return;
       		//注意下句设置背景底色为白色
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.White, 1.0f, 0);
            device.RenderState.CullMode = Cull.None;		//背面剔除，参见5.9节
            device.BeginScene();	//开始渲染
            device.VertexFormat = CustomVertex.TransformedColored.Format;
            Modify(0.0f, 0.0f);
            device.DrawUserPrimitives(PrimitiveType.TriangleFan, 5, verts1); 	//绘制5个三角形
            Modify(250.0f, 0.0f);
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 4, verts1); //绘制4个三角形
            Modify(500.0f, 0.0f);
            device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, verts1); //绘制2个三角形
            Modify(0.0f, 250.0f);
            device.DrawUserPrimitives(PrimitiveType.LineList, 3, verts1); 	//绘制3条线段
            Modify(250.0f, 250.0f);
            device.DrawUserPrimitives(PrimitiveType.LineStrip, 5, verts1);	//绘制5条线段
            Modify(500.0f, 250.0f);
            device.DrawUserPrimitives(PrimitiveType.PointList, 6, verts1);	//绘制6个点
            verts1 = null;
            device.EndScene();		//渲染结束
            device.Present();		//更新显示区域，把后备缓存的3D图形送到图形卡的显存中显示

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
            //Show();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Render();
        }

        void Modify(float x1, float y1)
        {
            verts1 = (CustomVertex.TransformedColored[])verts.Clone();
            for (int i = 0; i < 6; i++)
            {
                verts1[i].X += x1;
                verts1[i].Y += y1;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            Show();
            Render();
        }
    }
}
