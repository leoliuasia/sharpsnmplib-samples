using System;
using System.Collections.Generic;
using System.Net;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;

namespace Lextm.SharpSnmpLib.Browser
{
    class SecureAgentProfile : AgentProfile
    {
        private readonly IAuthenticationProvider _auth;
        private readonly IPrivacyProvider _priv;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("Lextm.SharpSnmpLib.Browser");
        private readonly ProviderPair _record;

        public SecureAgentProfile(Guid id, VersionCode version, IPEndPoint agent, string agentName, string authenticationPassphrase, string privacyPassphrase, int authenticationMethod, int privacyMethod, string userName)
            : base(id, version, agent, agentName, userName)
        {
            AuthenticationPassphrase = authenticationPassphrase;
            PrivacyPassphrase = privacyPassphrase;
            AuthenticationMethod = authenticationMethod;
            PrivacyMethod = privacyMethod;

            switch (AuthenticationMethod)
            {
                case 0:
                    _auth = DefaultAuthenticationProvider.Instance;
                    break;
                case 1:
                    _auth = new MD5AuthenticationProvider(new OctetString(AuthenticationPassphrase));
                    break;
                case 2:
                    _auth = new SHA1AuthenticationProvider(new OctetString(AuthenticationPassphrase));
                    break;
            }

            switch (PrivacyMethod)
            {
                case 0:
                    _priv = DefaultPrivacyProvider.Instance;
                    break;
                case 1:
                    _priv = new DESPrivacyProvider(new OctetString(PrivacyPassphrase), _auth);
                    break;
                case 2:
                    _priv = new AESPrivacyProvider(new OctetString(PrivacyPassphrase), _auth);
                    break;
            }

            _record = new ProviderPair(_auth, _priv);
        }

        internal string AuthenticationPassphrase { get; private set; }
        internal string PrivacyPassphrase { get; private set; }
        internal int AuthenticationMethod { get; private set; }
        internal int PrivacyMethod { get; private set; }

        internal override void Get(Manager manager, Variable variable)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                Logger.Info("User name need to be specified for v3.");
                return;
            }

            Discovery discovery = new Discovery(Messenger.NextMessageId, Messenger.NextRequestId);
            ReportMessage report = discovery.GetResponse(manager.Timeout, Agent);

            GetRequestMessage request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(UserName), new List<Variable> { variable }, _record, report);

            ISnmpMessage response = request.GetResponse(manager.Timeout, Agent);
            if (response.Pdu.ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create(
                    "error in response",
                    Agent.Address,
                    response);
            }

            Logger.Info(response.Pdu.Variables[0].ToString(manager.Objects));
        }

        internal override string GetValue(Manager manager, Variable variable)
        {
            Discovery discovery = new Discovery(Messenger.NextMessageId, Messenger.NextRequestId);
            ReportMessage report = discovery.GetResponse(manager.Timeout, Agent);

            GetRequestMessage request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(UserName), new List<Variable> { variable }, _record, report);

            ISnmpMessage response = request.GetResponse(manager.Timeout, Agent);
            if (response.Pdu.ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create(
                    "error in response",
                    Agent.Address,
                    response);
            }

            return response.Pdu.Variables[0].Data.ToString();
        }

        internal override void GetNext(Manager manager, Variable variable)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                Logger.Info("User name need to be specified for v3.");
                return;
            }

            Discovery discovery = new Discovery(1, 101);
            ReportMessage report = discovery.GetResponse(manager.Timeout, Agent);

            GetNextRequestMessage request = new GetNextRequestMessage(VersionCode.V3, 100, 0, new OctetString(UserName), new List<Variable> { variable }, _record, report);

            ISnmpMessage response = request.GetResponse(manager.Timeout, Agent);
            if (response.Pdu.ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create(
                    "error in response",
                    Agent.Address,
                    response);
            }

            Logger.Info(response.Pdu.Variables[0].ToString(manager.Objects));
        }

        internal override void Set(Manager manager, Variable variable)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                Logger.Info("User name need to be specified for v3.");
                return;
            }

            Discovery discovery = new Discovery(Messenger.NextMessageId, Messenger.NextRequestId);
            ReportMessage report = discovery.GetResponse(manager.Timeout, Agent);

            SetRequestMessage request = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(UserName), new List<Variable> { variable }, _record, report);

            ISnmpMessage response = request.GetResponse(manager.Timeout, Agent);
            if (response.Pdu.ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create(
                    "error in response",
                    Agent.Address,
                    response);
            }

            Logger.Info(response.Pdu.Variables[0].ToString(manager.Objects));
        }

        internal override void GetTable(Manager manager, IDefinition def)
        {
            //IList<Variable> list = new List<Variable>();
            //int rows = Messenger.Walk(VersionCode, Agent, new OctetString(GetCommunity), new ObjectIdentifier(def.GetNumericalForm()), list, manager.Timeout, WalkMode.WithinSubtree);
			
            //// 
            //// How many rows are there?
            ////
            //if (rows > 0)
            //{
            //    FormTable newTable = new FormTable(def);
            //    newTable.SetRows(rows);
            //    newTable.PopulateGrid(list);
            //    newTable.Show();
            //}
            //else
            //{
            //    TraceSource source = new TraceSource("Browser");
            //    foreach (Variable t in list)
            //    {
            //        source.TraceInformation(t.ToString());
            //    }

            //    source.Flush();
            //    source.Close();
            //}
        }

        public override void Walk(Manager manager, IDefinition definition)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                Logger.Info("User name need to be specified for v3.");
                return;
            }

            Discovery discovery = new Discovery(Messenger.NextMessageId, Messenger.NextRequestId);
            ReportMessage report = discovery.GetResponse(manager.Timeout, Agent);
            IList<Variable> list = new List<Variable>();
            Messenger.BulkWalk(VersionCode.V3, Agent, new OctetString(UserName),
                               new ObjectIdentifier(definition.GetNumericalForm()), list, manager.Timeout, 10,
                               WalkMode.WithinSubtree, _record, report);

            foreach (Variable v in list)
            {
                Logger.Info(v.ToString(manager.Objects));
            }
        }
    }
}
