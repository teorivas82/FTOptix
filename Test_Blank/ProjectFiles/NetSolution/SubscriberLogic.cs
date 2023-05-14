#region StandardUsing
using System;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.OPCUAServer;
#endregion
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using FTOptix.Report;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.DataLogger;
using FTOptix.ODBCStore;
using FTOptix.WebUI;
using FTOptix.Alarm;
using FTOptix.EventLogger;

public class SubscriberLogic : BaseNetLogic
{
    public override void Start()
    {
        var brokerIpAddressVariable = Project.Current.GetVariable("Model/BrokerIpAddress");

        // Create a client connecting to the broker (default port is 1883)
        subscribeClient = new MqttClient(brokerIpAddressVariable.Value);
        // Connect to the broker
        subscribeClient.Connect("SubscriberClient");
        // Assign a callback to be executed when a message is received from the broker
        subscribeClient.MqttMsgPublishReceived += SubscribeClientMqttMsgPublishReceived;
        // Subscribe to the "my_topic" topic with QoS 2
        ushort msgId = subscribeClient.Subscribe(new string[] { "/my_topic" }, // topic
            new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }); // QoS level

        messageVariable = Project.Current.GetVariable("Model/Message");
        messageTopic = Project.Current.GetVariable("Model/Topic");
    }

    public override void Stop()
    {
        subscribeClient.Unsubscribe(new string[] { "/my_topic" });
        subscribeClient.Disconnect();
    }

    private void SubscribeClientMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        messageVariable.Value = "Message received: " + System.Text.Encoding.UTF8.GetString(e.Message);
        messageTopic.Value = "Message received: " + System.Text.Encoding.UTF8.GetString(e.Message);
    }

    [ExportMethod]
    public void SubscribeTopic()
    {
        // Subscribe to the "my_topic" topic with QoS 2

        var newtopic = Project.Current.GetVariable("Model/NewTopic");

        ushort msgId = subscribeClient.Subscribe(new string[] {newtopic.Value}, // topic
            new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }); // QoS level
    }

    private MqttClient subscribeClient;
    private IUAVariable messageVariable;
    private IUAVariable messageTopic;
}
