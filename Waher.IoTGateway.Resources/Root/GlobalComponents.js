function NativeHeaderHandler() {
    const header = document.getElementById("native-header");
    let count = 0
    Array.from(header.children[0].children[1].getElementsByTagName("ul")).forEach(subMenu => {
        let topSubmenue = subMenu
        const menueItemText = subMenu.nextElementSibling || subMenu.previousElementSibling;

        while (topSubmenue.parentElement.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav") {
            topSubmenue = topSubmenue.parentElement.parentElement
        }

        if (window.matchMedia("screen and (max-width: 900px)").matches) {
            const button = document.createElement("button")
            button.classList.add("native-dropdown-button")
            button.addEventListener("click", () => {
                if (subMenu.toggleAttribute("expanded"))
                    button.style.rotate = "180deg"
                else    
                    button.style.rotate = "0deg"
            })
            subMenu.parentElement.insertBefore(button, subMenu.previousElementSibling)

            if (menueItemText.tagName === "P") {
                menueItemText.addEventListener("click", event => {
                    event.preventDefault();
                    if (subMenu.toggleAttribute("expanded"))
                        button.style.rotate = "180deg"
                    else    
                        button.style.rotate = "0deg"
                })
            }
        }

        // expand submenu
        subMenu.parentElement.addEventListener("mouseenter", () => {
            const delay = subMenu === topSubmenue ? 0 : 100

            setTimeout(() => {
                if (!subMenu.parentElement.matches(":hover"))
                    return;

                count++

                // cancled when on mobile view
                if (window.matchMedia("screen and (max-width: 900px)").matches)
                    return

                subMenu.setAttribute("expanded", "")

                // set set max height to prevent vertical overflow
                if (subMenu.getBoundingClientRect().bottom > window.innerHeight) {
                    subMenu.style.height = `${window.innerHeight - subMenu.getBoundingClientRect().top}px`
                }

                const html = document.getElementsByTagName("html")[0]
                const pageWidth = html.clientWidth

                // if overflowing to the righ, offset the top submenue to the left to not
                if (topSubmenue.offsetWidth + topSubmenue.offsetLeft > pageWidth) {
                    topSubmenue.style.left = `${pageWidth - topSubmenue.offsetWidth - 10}px`
                }

                // normalise list item width
                const textElements = []
                maxWidth = 0
                for (let i = 0; i < subMenu.children.length; i++) {
                    const child = subMenu.children[i]
                    const textElement = Array.from(child.children).find(c => c.tagName === "A" || c.tagName === "P")
                    if (textElement) {
                        const width = Number(window.getComputedStyle(textElement).width.split("px")[0])
                        maxWidth = Math.max(maxWidth, width)
                        textElements.push(textElement)
                    }
                }
                textElements.forEach(el => el.style.width = `${Math.ceil(maxWidth)}px`)

                // fix item heights
                const menueItems = subMenu.children
                for (let i = 0; i < menueItems.length; i++) {
                    if (menueItems[i].children[0])
                        menueItems[i].children[0].style.height = Math.ceil(menueItems[i].getBoundingClientRect().height) + "px"
                }

                // offset sibling elements (underneeth) to position over to not have gaps between list items
                const offsetParentsSiblings = (listItem) => {
                    const text = listItem.children[0]
                    listItem.style.height = "";
                    const siblingHeightOffset = listItem.getBoundingClientRect().height - text.getBoundingClientRect().height

                    let listItemSibling = listItem.nextElementSibling
                    while (listItemSibling) {
                        listItemSibling.style.transform = `translateY(-${siblingHeightOffset}px)`
                        listItemSibling = listItemSibling.nextElementSibling
                    }
                }

                let listItem = subMenu.parentElement
                while (true) {
                    if (listItem.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav") {
                        offsetParentsSiblings(listItem)
                        listItem = listItem.parentElement.parentElement
                    }
                    else
                        break
                }
            }, delay)

        })

        // close submenu
        subMenu.parentElement.addEventListener("mouseleave", () => {
            if (!subMenu.hasAttribute("expanded"))
                return
                
            count--
            if (count < 1) {
                topSubmenue.style.left = ""
            }
            // to use this you "need" a mouse
            if (!matchMedia('(pointer:fine)').matches)
                return

            subMenu.removeAttribute("expanded")


            // re-offset siblings to fill the space the closing of the submenue created
            const offsetParentsSiblings = (listItem) => {
                const text = listItem.children[0]
                const siblingHeightOffset = listItem.offsetHeight - text.offsetHeight

                let listItemSibling = listItem.nextElementSibling
                while (listItemSibling) {
                    listItemSibling.style.transform = `translateY(-${siblingHeightOffset}px)`
                    listItemSibling = listItemSibling.nextElementSibling
                }
            }

            let listItem = subMenu.parentElement
            while (true) {
                subMenu.style.height = ""
                if (listItem.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav") {
                    offsetParentsSiblings(listItem)
                    listItem = listItem.parentElement.parentElement
                }
                else
                    break
            }
        })
    })

    function ToggleNav() {
        header.toggleAttribute("data-visible");
    }

    return {
        ToggleNav
    }
}

function NativeFaviconHandler() {
    let faviconDotActive = false
    const favicon = document.querySelector("link[rel~='icon']");
    const originalFavicon = favicon.href

    function RemoveFaviconDot() {
        if (!faviconDotActive)
            return;

        faviconDotActive = false

        favicon.href = originalFavicon
    }

    function AddFaviconDot() {
        if (faviconDotActive)
            return;

        faviconDotActive = true

        let canvas = document.createElement("canvas");
        let ctx = canvas.getContext("2d");

        let img = new Image();
        img.src = favicon.href;
        img.onload = () => {
            // Set canvas size to match the favicon
            const dotRadius = img.width / 4.0;
            const dotMargin = img.width / 16.0
            canvas.width = img.width;
            canvas.height = img.height;

            // Draw the original favicon
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

            // Draw a red dot (top-right corner)
            ctx.fillStyle = "red";
            ctx.beginPath();
            ctx.arc(canvas.width - dotRadius - dotMargin, dotRadius + dotMargin, dotRadius, 0, 2 * Math.PI); // Adjust position/size if needed
            ctx.fill();

            // Replace the favicon
            favicon.href = canvas.toDataURL("image/png")
        };
    }

    return {
        AddFaviconDot,
        RemoveFaviconDot,
    }
}

function NativeBackdropHandler() {
    const BACKDROP_ID = "native-backdrop"

    // create backdrop
    const backdrop = document.createElement("div")
    backdrop.id = BACKDROP_ID
    document.body.appendChild(backdrop);
    HideBackdrop()

    function ShowBackdrop() {
        backdrop.style.display = "block"
    }

    function HideBackdrop() {
        backdrop.style.display = "none"
    }

    return {
        ShowBackdrop,
        HideBackdrop
    }
}

function PopupHandler() {
    const POPUP_CONTAINER_ID = "native-popup-container"
    const PROMPT_INPUT_ID = "native-prompt-input"
    const KEYCODE_TAB = 9;

    const popupStack = [];

    const popupContainer = document.getElementById(POPUP_CONTAINER_ID)
    popupContainer.tabIndex = "-1"
    popupContainer.role = "dialog"
    popupContainer.setAttribute("aria-labelledby", "native-popup-label")
    popupContainer.setAttribute("aria-describedby", "native-popup-description")

    let focusFunction = null;

    popupContainer.addEventListener("keydown", event => {
        if (event.key === "Enter") {
            const enterFunction = GetPopupProperty("enterFunction")
            if (enterFunction) {
                if (enterFunction() !== false)
                    event.preventDefault()
            }
        }
        if (event.key === "Escape") {
            const escapeFunction = GetPopupProperty("escapeFunction")
            if (escapeFunction) {
                if (escapeFunction() !== false)
                    event.preventDefault()
            }
        }
    })

    function ActivePopup() {
        if (popupStack.length < 1)
            return undefined
        return popupStack[popupStack.length - 1];
    }

    function GetPopupProperty(property) {
        const activePoup = ActivePopup()
        return activePoup ? activePoup[property] : undefined
    }

    function PopStack(args) {
        popupStack.pop().OnPop(args)
        DisplayPopup()
    }

    function DisplayPopup() {
        const activePopup = ActivePopup()
        if (!activePopup) {
            popupContainer.innerHTML = ""
            NativeFavicon.RemoveFaviconDot()
            NativeBackdrop.HideBackdrop()
            RemoveFocusTrap, 0
            return
        }

        popupContainer.innerHTML = activePopup.html
        UpdateFocusTrap()
        if (activePopup["OnShow"])
            activePopup["OnShow"]()
        NativeBackdrop.ShowBackdrop();
        NativeFavicon.AddFaviconDot();
    }

    function UpdateFocusTrap() {
        const focusableElements = popupContainer.querySelectorAll(
            `
            a[href]:not([disabled]),
            button:not([disabled]),
            textarea:not([disabled]),
            input:not([disabled]),
            select:not([disabled]),
            [tabindex]:not([tabindex="-1"]),
            [contenteditable]:not([contenteditable="false"])
            `
        );
        const firstFocusableElement = focusableElements[0];
        const lastFocusableElement = focusableElements[focusableElements.length - 1];

        if (focusFunction !== null)
            popupContainer.removeEventListener("keydown", focusFunction)

        focusFunction = (e) => {
            const isTabPressed = (e.key === 'Tab' || e.keyCode === KEYCODE_TAB);
            if (!isTabPressed)
                return;

            if (e.shiftKey) { // focus forward
                if (document.activeElement === firstFocusableElement) {
                    lastFocusableElement.focus();
                    e.preventDefault();
                }
            } else { // focus back
                if (document.activeElement === lastFocusableElement) {
                    firstFocusableElement.focus();
                    e.preventDefault();
                }
            }
        }

        popupContainer.addEventListener('keydown', focusFunction)
    }

    function RemoveFocusTrap() {
        if (focusFunction !== null)
            popupContainer.removeEventListener("keydown", focusFunction)
        focusFunction = null;
    }

    function SoftUpdate(html, uuid) {
        for (let i = 0; i < popupStack.length; i++) {
            if (popupStack[i].uuid === uuid) {
                updated = true
                popupStack[i].html = html
                return true
            }
        }
        return false
    }

    function PopupObject(html, enterFunction, escapeFunction, uuid, OnPop, OnShow) {
        return {
            html: html,
            enterFunction: enterFunction,
            escapeFunction: escapeFunction,
            uuid: uuid,
            OnPop: OnPop,
            OnShow: OnShow
        }
    }

    function Popup(html, popupArgs) {
        const uuid = crypto.randomUUID()
        return {
            uuid: uuid,
            userActionPromise: new Promise((resolve, reject) => {
                popupStack.push(PopupObject(
                    html,
                    popupArgs.enterFunction,
                    popupArgs.escapeFunction,
                    uuid,
                    (args) => resolve(args),
                    popupArgs.OnShow
                ))
                DisplayPopup()
            })
        }
    }

    function Focus() {
        popupContainer.focus()
    }

    /////////// Alert

    async function Alert(message, returnControlObject = false) {
        const html = CreateHTMLAlertPopup({ Message: `<p id="native-popup-message">${message}</p>` });

        const controlObject = Popup(html, {
            enterFunction: AlertOk,
            escapeFunction: AlertOk,
            OnShow: Focus,
        });

        if (!returnControlObject)
            return await controlObject.userActionPromise

        controlObject.PushUpdate = (newMessage) => {
            if (ActivePopup() && ActivePopup().uuid === controlObject.uuid)
                document.getElementById("native-popup-message").innerText = newMessage
            return SoftUpdate(CreateHTMLAlertPopup({ Message: `<p id="native-popup-message">${newMessage}</p>` }), controlObject.uuid)
        }

        return controlObject
    }
    function AlertOk() {
        PopStack()
    }

    /////////// Confirm

    async function Confirm(message, returnControlObject = false) {
        const html = CreateHTMLConfirmPopup({ Message: `<p id="native-popup-message">${message}</p>` });

        const controlObject = Popup(html, {
            enterFunction: () => {
                if (document.activeElement.tagName === "BUTTON")
                    return false
                ConfirmYes()
            },
            escapeFunction: ConfirmNo,
            OnShow: Focus,
        });

        if (!returnControlObject)
            return await controlObject.userActionPromise

        controlObject.PushUpdate = (newMessage) => {
            if (ActivePopup() && ActivePopup().uuid === controlObject.uuid)
                document.getElementById("native-popup-message").innerText = newMessage
            return SoftUpdate(CreateHTMLConfirmPopup({ Message: `<p id="native-popup-message">${newMessage}</p>` }), controlObject.uuid)
        }

        return controlObject
    }

    function ConfirmYes() {
        PopStack(true)
    }

    function ConfirmNo() {
        PopStack(false)
    }

    /////////// Prompt

    async function Prompt(message, returnControlObject = false) {
        const html = CreateHTMLPromptPopup({ Message: `<p id="native-popup-message">${message}</p>` });

        const controlObject = Popup(html, {
            enterFunction: () => PromptSubmit(document.getElementById('native-prompt-input').value),
            escapeFunction: () => PromptSubmit(undefined),
            OnShow: () => document.getElementById(PROMPT_INPUT_ID).focus()
        });

        if (!returnControlObject)
            return await controlObject.userActionPromise

        controlObject.PushUpdate = (newMessage) => {
            if (ActivePopup() && ActivePopup().uuid === controlObject.uuid)
                document.getElementById("native-popup-message").innerText = newMessage
            return SoftUpdate(CreateHTMLPromptPopup({ Message: `<p id="native-popup-message">${newMessage}</p>` }), controlObject.uuid)
        }

        return controlObject
    }

    async function PromptSubmit(value) {
        PopStack(value)
    }

    return {
        Alert,
        AlertOk,
        Confirm,
        ConfirmYes,
        ConfirmNo,
        Prompt,
        PromptSubmit,
    }
}

function Carousel(containerId) {
    const elementChangedEvent = new Event("elementchanged")

    let carouselIndex = 0;
    let carouselCount = 0;
    let height = 0

    const previousButton = document.querySelector(`*[data-carousel=${containerId}][data-carousel-previous]`);
    const nextButton = document.querySelector(`*[data-carousel=${containerId}][data-carousel-next]`);
    const carouselContainer = document.getElementById(containerId);
    const carouselButtons = Array.from(document.querySelectorAll(`[data-carousel=${containerId}][data-carousel-button]`))


    if ([previousButton, nextButton, carouselContainer].includes(undefined)) {
        Popup.Alert(`Carousel (${containerId}) is not properly setup`);
        return;
    }

    carouselCount = carouselContainer.children.length;

    function BoundedIndex(index) {
        return (index + carouselCount /* + carouselCount to cover cases where you take the index - 1 and it becomes -1 wich would return a negative modulo */) % carouselCount
    }

    function SetCarouselIndex(index) {
        console.log(1)
        if (carouselButtons.length > 0) {
            if (carouselIndex >= 0)
                carouselButtons[carouselIndex].classList.remove("buttonSelected")
            carouselButtons[index].classList.add("buttonSelected")
        }
        carouselIndex = index
    }

    function SetCarouselIndexAnim(index) {
        if (index === carouselIndex)
            return

        if (carouselCount > 1) {

            const dir = Math.sign(index - carouselIndex)
            if (dir === 1) {
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].removeAttribute("data-carousel-left");
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].style.transition = "none"
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].setAttribute("data-carousel-right", "");
                carouselContainer.offsetLeft; // flush css changes in order to skip the transition
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].style.transition = ""
                carouselContainer.children[carouselIndex].removeAttribute("data-carousel-active");
                carouselContainer.children[carouselIndex].setAttribute("data-carousel-left", "");
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].removeAttribute("data-carousel-right", "")
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].setAttribute("data-carousel-active", "")
            } else {
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].removeAttribute("data-carousel-right");
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].style.transition = "none"
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].setAttribute("data-carousel-left", "");
                carouselContainer.offsetLeft; // flush css changes in order to skip the transition
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].style.transition = ""
                carouselContainer.children[carouselIndex].removeAttribute("data-carousel-active");
                carouselContainer.children[carouselIndex].setAttribute("data-carousel-right", "");
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].removeAttribute("data-carousel-left", "")
                carouselContainer.children[BoundedIndex(carouselIndex + 1)].setAttribute("data-carousel-active", "")
            }
            SetCarouselIndex(BoundedIndex(carouselIndex + dir));
        }


        carouselContainer.dispatchEvent(elementChangedEvent)
    }

    // needs to be run when the height of any elements in the carousel changes
    function CalibrateHeight() {
        let max = 0;
        Array.from(carouselContainer.children).forEach(x => {
            x.style.bottom = -1000;
            x.style.right = -1000;
            x.style.display = "block"
            max = Math.max(max, x.getBoundingClientRect().height)
            x.style.display = ""
        })
        if (max !== height) {
            height = max;
            carouselContainer.style.height = max + "px";
        }
    }

    function Initialize() {
        if (previousButton && nextButton) {
            previousButton.addEventListener("click", () => SetCarouselIndexAnim(carouselIndex - 1));
            nextButton.addEventListener("click", () => SetCarouselIndexAnim(carouselIndex + 1));
        }
        else {
            carouselButtons.forEach(e => {
                e.addEventListener("click", () => {
                    SetCarouselIndexAnim(Number(e.getAttribute("data-carousel-button")))
                })
            })
        }

        CalibrateHeight();

        carouselIndex = -1
        for (let i = 0; i < carouselContainer.children.length; i++) {
            if (carouselContainer.children[i].hasAttribute("data-carousel-active")) {
                carouselContainer.children[i].setAttribute("data-carousel-active", "")
                SetCarouselIndex(i);
                break;
            }
        }

        if (carouselIndex === -1) {
            carouselContainer.children[0].setAttribute("data-carousel-active", "")
            SetCarouselIndex(0);
        }

        if (carouselCount < 2) {
            if (previousButton)
                previousButton.remove()
            if (nextButton)
                nextButton.remove()
        }

    }

    Initialize()

    return {
        get current() { return carouselContainer.children[carouselIndex] },
        get container() { return carouselContainer },

        CalibrateHeight,
    }
}

let Popup;
let NativeHeader;
let NativeFavicon;
let NativeBackdrop;

window.addEventListener("load", () => {
    NativeHeader = NativeHeaderHandler();
    Popup = PopupHandler();
    NativeFavicon = NativeFaviconHandler();
    NativeBackdrop = NativeBackdropHandler()

    const largeName = document.getElementById("large-pagpage-name");
    if (largeName !== null)
        largeName.parentElement.style.flex = 1;
})

