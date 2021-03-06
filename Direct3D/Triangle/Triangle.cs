﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Triangle
{
    public partial class Triangle : Form
    {
        private Device device = null;
        CustomVertex.TransformedColored[] verts;
        public Triangle()
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
            verts = new CustomVertex.TransformedColored[3];
            verts[0].Position = new Vector4(150.0f, 50.0f, 0.5f, 1.0f);//三角形的第1个顶点坐标
            verts[0].Color = Color.Aqua.ToArgb();				//三角形的第1个顶点颜色
            verts[1].Position = new Vector4(250.0f, 250.0f, 0.5f, 1.0f); //第2个顶点坐标
            verts[1].Color = Color.Brown.ToArgb();
            verts[2].Position = new Vector4(50.0f, 250.0f, 0.5f, 1.0f); //第3个顶点坐标
            verts[2].Color = Color.LightPink.ToArgb();
        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Render();
        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 	//如果未建立设备对象，退出
                return;
            //下边函数将显示区域初始化为蓝色，第1个参数指定要初始化目标窗口
            //第2个参数是我们所要填充的颜色。第3、第4个参数一般为1.0f, 0。
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer,
        System.Drawing.Color.Blue, 1.0f, 0);
            device.BeginScene();	//开始渲染
            //渲染代码必须放在device.BeginScene()和device.Present()之间
            device.VertexFormat = CustomVertex.TransformedColored.Format;		 //渲染代码
            device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, verts);
            device.EndScene();		//渲染结束
            device.Present();		//更新显示区域，把后备缓存的3D图形送到图形卡的显存中显示
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            Show();
            Render();
        }
    }
}
