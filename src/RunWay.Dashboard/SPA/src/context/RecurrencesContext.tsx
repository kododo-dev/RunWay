import { createContext, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { useApi } from '../api/api';

interface RecurrencesContextValue {
    totalCount: number;
}

const RecurrencesContext = createContext<RecurrencesContextValue>({
    totalCount: 0,
});

export const RecurrencesProvider = ({ children }: { children: ReactNode }) => {
    const api = useApi();
    const [totalCount, setTotalCount] = useState(0);

    useEffect(() => {
        let mounted = true;
        const load = async () => {
            try {
                const data = await api.fetchRecurrences(1);
                if (mounted) setTotalCount(data.totalItems || 0);
            } catch {}
        };
        load();
        const interval = setInterval(load, 30000);
        return () => {
            mounted = false;
            clearInterval(interval);
        };
    }, [api]);

    return (
        <RecurrencesContext.Provider value={{ totalCount }}>
            {children}
        </RecurrencesContext.Provider>
    );
};

export const useRecurrences = () => useContext(RecurrencesContext);
