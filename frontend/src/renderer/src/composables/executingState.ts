import { toRef, type Ref } from 'vue';

export function useExecutingState(current?: boolean | Ref<boolean>) {
    const isExecuting = toRef(current ?? false);

    async function execute<T>(action: () => PromiseLike<T>) {
        isExecuting.value = true;
        try {
            return await action();
        } finally {
            isExecuting.value = false;
        }
    }

    return { isExecuting, execute };
}
