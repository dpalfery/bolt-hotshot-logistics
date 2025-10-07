'use client';

import { LoginButton } from '@/components/auth/LoginButton';
import { useIsAuthenticated } from '@azure/msal-react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useEffect } from 'react';

export default function LoginPage() {
    const isAuthenticated = useIsAuthenticated();
    const router = useRouter();
    const searchParams = useSearchParams();

    useEffect(() => {
        if (isAuthenticated) {
            const redirectUri = searchParams.get('redirect_uri');
            router.push(redirectUri || '/');
        }
    }, [isAuthenticated, router, searchParams]);

    return (
        <div className="flex h-screen w-full items-center justify-center bg-gray-100">
            <div className="w-full max-w-sm p-8 bg-white rounded-lg shadow-md">
                <h1 className="text-2xl font-bold text-center text-gray-800 mb-6">Hotshot Logistics</h1>
                <p className="text-center text-gray-600 mb-8">Please log in to continue</p>
                <LoginButton />
            </div>
        </div>
    );
}