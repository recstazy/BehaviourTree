using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Base task to derive if you want a variable count of outputs
    /// </summary>
    [ExcludeFromTaskSelector]
    public abstract class MultioutTask : BehaviourTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        internal override TaskConnection[] PostProcessConnectionsAfterChange(TaskConnection[] connections)
        {
            if (connections == null || connections.Length == 0) return connections;

            int concreteOutsCount = GetType().GetCustomAttributes(typeof(TaskOutAttribute), true).Length;
            TaskConnection[] reordableOuts = new TaskConnection[connections.Length - concreteOutsCount];

            for (int i = concreteOutsCount; i < connections.Length; i++)
            {
                reordableOuts[i - concreteOutsCount] = connections[i];
            }

            System.Array.Sort(reordableOuts, (c, next) => c.OutPin > next.OutPin ? 1 : -1);

            for (int i = 0; i < reordableOuts.Length; i++)
            {
                int oldPin = reordableOuts[i].OutPin;
                var newConnection = new TaskConnection(i + concreteOutsCount, reordableOuts[i].InNode);
                newConnection.OldOutPin = oldPin;
                reordableOuts[i] = newConnection;
            }

            reordableOuts.CopyTo(connections, concreteOutsCount);
            return connections;
        }
    }
}
