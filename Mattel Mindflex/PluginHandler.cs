using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace lucidcode.LucidScribe.Plugin.Mattel.Mindflex
{

  public static class Device
  {
    private static SerialPort serialPort;
    private static bool initialized;
    private static bool initError;

    private static int attention;
    private static int meditation;
    private static int lowAlpha;
    private static int highAlpha;
    private static int lowBeta;
    private static int highBeta;
    private static int lowGamma;
    private static int midGamma;
    private static int delta;
    private static int theta;

    private static int index = 0;
    private static int lastByte = -1;
    private static int value = 0;
    private static int[] packetData = new int[64]; 

    public static Boolean Initialize()
    {
      try
      {
        if (!initialized && !initError)
        {
          PortForm formPort = new PortForm();
          if (formPort.ShowDialog() == DialogResult.OK)
          {
            try
            {
              // Open the COM port
              serialPort = new SerialPort(formPort.SelectedPort);
              serialPort.BaudRate = 9600;
              serialPort.Parity = Parity.None;
              serialPort.DataBits = 8;
              serialPort.StopBits = StopBits.One;
              serialPort.Handshake = Handshake.None;
              serialPort.ReadTimeout = 500;
              serialPort.WriteTimeout = 500;
              serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
              serialPort.Open();
              initialized = true;
            }
            catch (Exception ex)
            {
              if (!initError)
              {
                MessageBox.Show(ex.Message, "LucidScribe.InitializePlugin()", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
              initError = true;
            }
          }
          else
          {
            initError = true;
            return false;
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        initError = true;
        throw (new Exception("The 'NeuroSky' plugin failed to initialize: " + ex.Message));
      }
    }

    static void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      try
      {
        while (serialPort.BytesToRead > 0)
        {
          int num = serialPort.ReadByte();
          if ((lastByte == 170) & (num == 170)) //0xAA = 170 decimal
          {
            index = 1;
          }
          lastByte = num;

          packetData[index] = num;
          // Sample data
          // 0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 (index)
          //aa aa 20 02 33 83 18 0b f3 7b 03 b2 1c 00 f4 56 00 f6 42 01 00 4a 01 0b e9 00 5d 19 00 6a 85 04 00 05 00 b5 
          //aa aa 20 02 33 83 18 04 d2 2b 00 32 5b 00 0c 56 00 08 49 00 04 f9 00 02 22 00 00 c3 00 00 7d 04 00 05 00 84 
          //aa aa 20 02 33 83 18 05 75 dc 00 97 ee 00 25 1f 00 9d f0 00 ac 67 00 8c 7d 00 3f 77 00 2e 74 04 00 05 00 06 
          //aa aa 20 02 33 83 18 0a db ef 11 72 b6 01 70 e4 03 16 4f 04 96 7d 06 b7 6f 0b 71 d0 09 68 34 04 00 05 00 28 
          //aa aa 20 02 33 83 18 0a b2 21 03 09 50 00 ed 5f 01 df 51 02 c2 69 02 21 6a 00 6c 94 00 59 32 04 00 05 00 2b 
          //Notes
          //         02 flags signalQuality                                                                    05 flags meditation
                         //83 flags eeg power (8*3byte unsigned integers)                                04 flags attention
                                                                                                             



          if (index == 4)
          {
            //signalQuality = num;
              
          }

          if (index == 32)
          {
            attention = num * 4;
          }

          if (index == 34)
          {
            meditation = num * 4;
            ProcessPacket();
            index = -1; //soon to be 0
          }

          index++;
        }
      }
      catch (Exception ex)
      {
        try
        {
          serialPort.DataReceived -= serialPort_DataReceived;
          serialPort.Close();
        }
        catch (Exception ex2)
        {
        }
        MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "OpenEEG.DataReceived()", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static void ProcessPacket()
    { 
      int[] eegPower = new int[8];
      for (byte i = 0; i < 35; i++) {
        if (packetData[i] == 131)
        {
          for (int j = 0; j < 8; j++)
          {
            eegPower[j] = (packetData[++i] << 16) | (packetData[++i] << 8) | packetData[++i];
          }
        }
      }

      delta = eegPower[0] / 1000;
      theta = eegPower[1] / 1000;
      lowAlpha= eegPower[2] / 100;
      highAlpha = eegPower[3] / 100;
      lowBeta = eegPower[4] / 100;
      highBeta = eegPower[5] / 100;
      lowGamma = eegPower[6] / 100;
      midGamma = eegPower[7] / 100;
    }

    public static Double GetREM()
    {
      return 0;
    }

    public static Double GetAttention()
    {
      return attention;
    }

    public static Double GetMeditation()
    {
      return meditation;
    }

    public static Double GetLowAlpha()
    {
      return lowAlpha;
    }

    public static Double GetHighAlpha()
    {
      return highAlpha;
    }

    public static Double GetLowBeta()
    {
      return lowBeta;
    }

    public static Double GetHighBeta()
    {
      return highBeta;
    }

    public static Double GetLowGamma()
    {
      return lowGamma;
    }

    public static Double GetMidGamma()
    {
      return midGamma;
    }

    public static Double GetDelta()
    {
      return delta;
    }

    public static Double GetTheta()
    {
      return theta;
    }
  }

  namespace Delta
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Delta"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetDelta();
          if (dblValue > 999) { dblValue = 999; }
          if (dblValue < 0) { dblValue = 0; }
          return dblValue;
        }
      }
    }
  }

  namespace RapidEyeMovement
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      List<int> m_arrHistory = new List<int>();
      public override string Name
      {
        get { return "Mindfles REM"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          return 0;
        }
      }
    }
  }

  namespace Attention
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Attention"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetAttention();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace Meditation
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Meditation"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetMeditation();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace Theta
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Theta"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetTheta();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace LowAlpha
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Low Alpha"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetLowAlpha();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace HighAlpha
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "High Alpha"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetHighAlpha();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace LowBeta
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Low Beta"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetLowBeta();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace HighBeta
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "High Beta"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetHighBeta();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace LowGamma
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Low Gamma"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetLowGamma();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

  namespace MidGamma
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Mid Gamma"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetMidGamma();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
    }
  }

}
