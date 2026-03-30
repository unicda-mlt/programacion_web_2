'use client';
import { useState, useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

export interface FirstCandidacy {
  id: string;
  name: string;
  lastName: string;
  imageUrl: string;
}

export interface Slate {
  id: string;
  position: number;
  voteCount: number;
  firstCandidacy: FirstCandidacy | null;
}

export interface ScrutinyVoteStatus {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  imageUrl: string;
  totalVotes: number;
  slates: Slate[];
}

export interface VoteStatusData {
  scrutinies: ScrutinyVoteStatus[];
}

export type ConnectionStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

const HUB_URL = 'https://localhost:7192/hubs/vote-status';

export function useVoteStatus() {
  const [data, setData] = useState<VoteStatusData>({ scrutinies: [] });
  const [status, setStatus] = useState<ConnectionStatus>('disconnected');
  const [error, setError] = useState<string | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function start() {
      setStatus('connecting');
      setError(null);

      const res = await fetch('/api/auth/token');
      if (!res.ok) {
        if (!cancelled) {
          setStatus('disconnected');
          setError('Authentication required. Please log in again.');
        }
        return;
      }
      const { token } = await res.json() as { token: string | null };
      if (!token) {
        if (!cancelled) {
          setStatus('disconnected');
          setError('Authentication required. Please log in again.');
        }
        return;
      }

      const connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, { headers: { Authorization: `Bearer ${token}` } })
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      connection.on('ReceiveVoteStatus', (incoming: VoteStatusData) => {
        if (!cancelled) setData(incoming);
      });

      connection.onclose((err) => {
        if (cancelled) return;
        setStatus('disconnected');
        if (err) {
          if (err.message?.includes('401') || err.message?.includes('No autorizado')) {
            setError('Unauthorized: invalid or expired token.');
          } else {
            setError('Connection closed: ' + err.message);
          }
        }
      });

      connection.onreconnecting(() => {
        if (!cancelled) setStatus('reconnecting');
      });

      connection.onreconnected(() => {
        if (!cancelled) {
          setStatus('connected');
          connection.invoke('SubscribeToVoteUpdates').catch(() => {});
        }
      });

      connectionRef.current = connection;

      try {
        await connection.start();
        if (cancelled) { await connection.stop(); return; }
        setStatus('connected');
        await connection.invoke('SubscribeToVoteUpdates');
      } catch (err: unknown) {
        if (cancelled) return;
        setStatus('disconnected');
        const msg = err instanceof Error ? err.message : String(err);
        setError('Connection error: ' + msg);
      }
    }

    start();

    return () => {
      cancelled = true;
      const conn = connectionRef.current;
      if (conn) {
        conn.invoke('UnsubscribeFromVoteUpdates').catch(() => {});
        conn.stop().catch(() => {});
        connectionRef.current = null;
      }
    };
  }, []);

  return { data, status, error };
}
