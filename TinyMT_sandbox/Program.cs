using System;

namespace TinyMT_sandbox
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var status = new uint[4] { 0x5eadbeef, 0x12345678, 0x11235813, 0x31415926 };
            var param = new TinyMTParameter(0x8f7011ee, 0xfc78ff1f, 0x3793fdff);
            var tiny = new TinyMT(status, param);

            for (int i = 0; i < 10000; i++)
            {
                tiny.nextState();
            }

            Console.WriteLine("{0:X08} {1:X08} {2:X08} {3:X08}",
                                  tiny.status[0], tiny.status[1], tiny.status[2], tiny.status[3]);


            var poly = "d8524022ed8dff4a8dcc50c798faba43"; // characteristic polynomial which depends on TinyMTParameter value
            var tiny2 = new TinyMT(status, param);
            tiny2.jumpState(10000, 0, poly);
            Console.WriteLine("{0:X08} {1:X08} {2:X08} {3:X08}",
                                  tiny2.status[0], tiny2.status[1], tiny2.status[2], tiny2.status[3]);
        }
    }
}
