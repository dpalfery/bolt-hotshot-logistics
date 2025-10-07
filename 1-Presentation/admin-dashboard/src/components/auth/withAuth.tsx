'use client';

import { useIsAuthenticated } from '@azure/msal-react';
import { useRouter } from 'next/navigation';
import { useEffect, ComponentType } from 'react';

const withAuth = <P extends object>(WrappedComponent: ComponentType<P>) => {
  const WithAuthComponent = (props: P) => {
    const isAuthenticated = useIsAuthenticated();
    const router = useRouter();

    // Check for test mode bypass or development mode
    const isTestMode = typeof window !== 'undefined' && 
      (window as any).__BYPASS_AUTH__ === true;
    const isDevelopment = process.env.NODE_ENV === 'development';

    useEffect(() => {
      if (!isAuthenticated && !isTestMode && !isDevelopment) {
        router.push('/login');
      }
    }, [isAuthenticated, isTestMode, isDevelopment, router]);

    if (!isAuthenticated && !isTestMode && !isDevelopment) {
      return null;
    }

    return <WrappedComponent {...props} />;
  };

  return WithAuthComponent;
};

export default withAuth;
