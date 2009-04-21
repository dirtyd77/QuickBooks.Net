﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QBXMLRP2Lib;
using System.Xml.Linq;
using log4net;
using System.Reflection;
using QuickBooks.Net.Query;
using QuickBooks.Net.Reports;
using QuickBooks.Net.Domain;

namespace QuickBooks.Net
{
    public class QBSession : IQBSession, IQBSessionInternal
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected QBSessionFactory _sessionFactory;
        protected string _qbXmlVersion;
        protected string _ticket;

        public virtual bool IsOpen { get; protected set; }
        public virtual IQueries Queries { get; protected set; }
        public virtual IReports Reports { get; protected set; }

        internal QBSession(QBSessionFactory sessionFactory, string qbXmlVersion, string ticket)
        {
            _log.InfoFormat("Opening Session Ticket {0}", ticket);

            _sessionFactory = sessionFactory;
            _qbXmlVersion = qbXmlVersion;
            _ticket = ticket;
            IsOpen = true;
            Queries = new Queries(this);
            Reports = new Reports.Reports(this);
        }

        public XElement ProcessRequest(XElement QBXmlMsgsRq)
        {
            var doc = new XDocument(
                        new XProcessingInstruction("qbxml", string.Format("version=\"{0}\"", _qbXmlVersion)),
                        new XElement("QBXML",
                            new XElement("QBXMLMsgsRq", new XAttribute("onError", "stopOnError"),
                                QBXmlMsgsRq)));

            if (_log.IsDebugEnabled) 
                _log.Debug("QBXmlMsgsRq:\n" + doc.ToString()); 
       
            return _sessionFactory.ProcessRequest(_ticket, "<?xml version=\"1.0\"?>" + doc.ToString(SaveOptions.DisableFormatting));
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            _log.InfoFormat("Closing Session Ticket {0}", _ticket);
            _sessionFactory.CloseSession();
            IsOpen = false;
        }
    }
}