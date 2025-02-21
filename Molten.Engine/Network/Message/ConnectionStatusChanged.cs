﻿namespace Molten.Net.Message
{
    public abstract class ConnectionStatusChanged : INetworkMessage
    {
        public byte[] Data { get; }
        public int Sequence { get; }
        public DeliveryMethod DeliveryMethod { get; }

        protected ConnectionStatusChanged(byte[] data, DeliveryMethod deliveryMethod, int sequence)
        {
            Data = data;
            Sequence = sequence;
            DeliveryMethod = deliveryMethod;
        }

        public abstract INetworkConnection Connection { get; }
    }
}