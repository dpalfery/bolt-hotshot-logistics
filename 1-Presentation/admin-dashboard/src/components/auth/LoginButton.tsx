"use client";

import { useMsal } from "@azure/msal-react";

export const LoginButton = () => {
    const { instance } = useMsal();

    const handleLogin = () => {
        instance.loginRedirect().catch((e: Error) => {
            console.error(e);
        });
    };

    return (
        <button
            onClick={handleLogin}
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
        >
            Login
        </button>
    );
};