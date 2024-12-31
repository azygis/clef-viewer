import { isRef, MaybeRef } from 'vue';

export function useLimitedTextLength(maxLength: MaybeRef<number | null | undefined>) {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    function truncate(value: any) {
        if (typeof value !== 'string') {
            value = value.toString();
        }

        const limit = isRef(maxLength) ? maxLength.value : maxLength;
        // +6 so we don't show more characters than we actually limit
        // "… (+1)" takes more space than just putting that one remaining character
        if (!limit || limit < 1 || value.length <= limit + 6) {
            return value;
        }

        const remainder = value.length - limit;
        return `${value.slice(0, limit)}… (+${remainder})`;
    }

    return { truncate };
}
