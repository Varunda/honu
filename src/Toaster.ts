export type TextColor = "primary" | "secondary" | "success" | "danger" | "warning" | "info" | "body" | "muted";

export default class Toaster {

    private static _nextInstance: number = 1;

    public static remove(instanceID: number): void {
        //console.log(`Removing toast instance ${instanceID}`);
        $(`#toast-entry-${instanceID}`).empty();
        $(`#toast-entry-${instanceID}`).remove();
    }

    public static add(headerText: string, bodyText: string, headerColor: TextColor = "body"): void {
        const instId: number = Toaster._nextInstance++;

        const html: string = `
            <div class="toast m-3" data-delay="8000" id="toast-entry-${instId}" onclick="Toaster.remove(${instId})">
                <div class="toast-header" style="line-height: 1.75;" data-dismiss="toast">
                    <strong class="mr-auto text-${headerColor}">${headerText}</strong>
                    <button type="button" class="ml-2 mb-1 close" data-dismiss="toast">
                        <span>&times;</span>
                    </button>
                </div>
                <div class="toast-body">
                    ${bodyText}
                </div>
            </div>`;

        setTimeout(() => {
            Toaster.remove(instId);
        }, 8 * 1000);

        $("#toaster").append(html);
        $(".toast").toast("show");
    }

}

(window as any).Toaster = Toaster; // Necessary so they can be removed

