import { default as React } from 'react';
interface ConfirmModalProps {
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    onConfirm: () => void;
    onCancel: () => void;
    type?: 'danger' | 'warning' | 'info';
}
export declare const ConfirmModal: React.FC<ConfirmModalProps>;
export {};
