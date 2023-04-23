using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace StreamDeckBeamNG
{
    public sealed class OutGaugeReceiver
    {
        private static readonly Lazy<OutGaugeReceiver> lazy = new Lazy<OutGaugeReceiver>(() => new OutGaugeReceiver());

        public static OutGaugeReceiver Instance { get { return lazy.Value; } }

        private UdpClient receiver;
        public List<IOutGaugeSubscriber> Subscribers = new List<IOutGaugeSubscriber>();

        private OutGaugeReceiver() {
            receiver = new UdpClient(4444);
            receiver.BeginReceive(DataReceived, receiver);
        }

        private void DataReceived(IAsyncResult ar) {
            UdpClient c = (UdpClient)ar.AsyncState;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);

            PacketReader reader = new PacketReader(receivedBytes);

            OutGaugeData data = new OutGaugeData();

            reader.Skip(10);
            byte gear = reader.ReadByte();
            reader.Skip(1);
            data.speed = reader.ReadSingle();
            float rpm = reader.ReadSingle();

            foreach (var subscriber in Subscribers)
            {
                subscriber.OnOutGaugeDataReceived(data);
            }

            if (receiver != null)
            {
                c.BeginReceive(DataReceived, ar.AsyncState);
            }
        }
    }
}
