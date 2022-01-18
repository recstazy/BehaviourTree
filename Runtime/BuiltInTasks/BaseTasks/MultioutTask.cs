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

            // Find from witch connection we can start output recalculation
            System.Array.Sort(connections, (c, next) => c.OutPin > next.OutPin ? 1 : -1);
            int reordablesStartIndex = 0;
            int concreteOutsCount = GetType().GetCustomAttributes(typeof(TaskOutAttribute), true).Length;

            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].OutPin >= concreteOutsCount)
                {
                    reordablesStartIndex = i;
                    break;
                }
            }

            var reordableOuts = new TaskConnection[connections.Length - reordablesStartIndex];

            for (int i = reordablesStartIndex; i < connections.Length; i++)
            {
                reordableOuts[i - reordablesStartIndex] = connections[i];
            }

            for (int i = 0; i < reordableOuts.Length; i++)
            {
                int oldPin = reordableOuts[i].OutPin;
                var newConnection = new TaskConnection(i + concreteOutsCount, reordableOuts[i].InNode);
                newConnection.OldOutPin = oldPin;
                reordableOuts[i] = newConnection;
            }

            reordableOuts.CopyTo(connections, reordablesStartIndex);
            return connections;
        }
    }
}
